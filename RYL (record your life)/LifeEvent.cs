using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RYL__record_your_life_
{
    /* 
     * Повторяемость события:
     * Очень редко (вплоть до "раз в жизни"),
     * Иногда,
     * Постоянно
     */
    public enum Repeatability
    {
        Once,
        Sometimes,
        Constantly
    }
    /*
     * Период времени. Необходимо для построения графика за указанный период:
     * За день,
     * За месяц,
     * За год
     */
    public enum Period
    {
        Day,
        Month,
        Year
    }

    [Serializable]
    public struct Record
    {
        public Int32 ID { get; private set; }       // ID.
        public DateTime Date  { get; private set; } // Дата.
        private String  description;                // Описание.

        public Record(Int32 id, DateTime date, String description)
        {
            this = new Record();
            ID = id;
            Date = date;
            this.description = description ?? throw new ArgumentNullException("Null Argument");
        }

        public String Description
        {
            get
            {
                return description;
            }
            set
            {
                if (description == null) throw new ArgumentNullException("Null argument.");
                description = value;
            }
        }
    }

    [Serializable]
    public sealed class LifeEvent : ICloneable
    {
        public Int32 ID           { get; private set; } // ID.
        public String Name        { get; private set; } // Имя.
        public String Description { get; private set; } // Описание.
        public Repeatability Rate { get; private set; } // Коэффициент повторяемости (см. перечисление Repeatability).

        private List<Record> records;                   // Все записи.

        /// <param name="id">ID события.</param>
        /// <param name="name">Название.</param>
        /// <param name="description">Описание.</param>
        /// <param name="rate">Повторяемость события.</param>
        public LifeEvent(Int32 id, String name, String description, Repeatability rate)/* : this()*/
        {
            if (name == null || description == null)
                throw new ArgumentNullException("Null argument.");
            if (id < 0 || name.Length < 1)
                throw new ArgumentException("Invalid argument.");

            ID = id;
            Name = name;
            Description = description;
            Rate = rate;
            records = new List<Record>();
        }

        /// <summary>
        /// Добавление записи о произошедем событии.
        /// </summary>
        /// <param name="record">Запись.</param>
        public void AddRecord(DateTime date, String description)
        {
            // Если дату не определели, значит запись "холостая" (дата должна автоматически определяться во время добавления)
            if (date == new DateTime())
                throw new ArgumentException("Invalid date.");
            records.Add(new Record(records.Count, date, description));
        }

        /// <summary>
        /// Удаление записи.
        /// </summary>
        /// <param name="id">ID записи.</param>
        public void DeleteRecord(Int32 id)
        {
            if (id < 0 || id >= records.Count)
                throw new ArgumentOutOfRangeException("Invalid index.");

            records.RemoveAt(id);
        }

        /// <summary>
        /// Изменение ID.
        /// </summary>
        /// <param name="newID">Новый ID.</param>
        public void ChangeID(Int32 newID) =>
            // На самом деле это ужасно, т.к. страдает инкапсуляция, но я пока не придумал ничего лучше.
            ID = newID;

        /// <summary>
        /// Поиск записей по дате или описанию.
        /// </summary>
        /// <param name="template">Шаблон дя поиска.</param>
        /// <returns>Массив записей, описание или дата создания которых совпадают с заданным шаблоном.</returns>
        public Record[] SearchInRecords(String template)
        {
            if (template == null) throw new ArgumentNullException("Null argument");
            if (records.Count < 1) return null;

            string pattern = "";

            if (template.Length < 1)
                pattern = "(^|.*)" + template + "(.*|$)";
            else
            {
                pattern = "(^|.*)";
                int i;
                foreach (char ch in template)
                {
                    // 48-57 - цифры
                    // 65-90, 97-122 - английские
                    // 1040-1071, 1072-1103 - русские
                    i = (int)ch;
                    if (!((i >= 48 && i <= 57) || (i >= 65 && i <= 90) || (i >= 97 && i <= 122) || (i >= 1040 && i <= 1071) || (i >= 1072 && i <= 1103)) &&
                        (ch != 'n' && ch != 't' && ch != '>' && ch != '<' && ch != '_'))
                        pattern += "\\" + ch;
                    else
                        pattern += ch;
                }
                pattern += "(.*|$)";
            }

            var searchRequest = records.
                Where(record => (Regex.Match(record.Date.ToShortDateString(), pattern, RegexOptions.IgnoreCase).Length > 0) || (Regex.Match(record.Description, pattern, RegexOptions.IgnoreCase).Length > 0)).
                Select(record => record);
            if (searchRequest.Count() > 0)
            {
                Record[] result = new Record[searchRequest.Count()];
                int i = 0;
                foreach (var record in searchRequest)
                    result[i++] = record;
                return result;
            }
            else
                return null;
        }

        /// <summary>
        /// Получение массива записей за указанный период.
        /// </summary>
        /// <param name="period">Перечисление период.</param>
        /// <returns>Массив записей.</returns>
        public Record[] GetRecordsOnPeriod(Period period)
        {
            Record[] result = new Record[0];
            Int32 index = 0;

            for (int i = 0; i < records.Count; i++)
            {
                Int32 countOfChangedTime = 1;
                if (i == records.Count - 1)
                {
                    Array.Resize<Record>(ref result, result.Length + 1);
                    result[index++] = records[i];
                    break;
                }
                int j = i + 1;
                for (; j < records.Count; j++)
                {
                    if (period == Period.Day)
                    {
                        if (records[i].Date.ToShortDateString() == records[j].Date.ToShortDateString())
                            countOfChangedTime++;
                        else
                            break;
                    }
                    else if (period == Period.Month)
                    {
                        if ((records[i].Date.Year == records[j].Date.Year) && (records[i].Date.Month == records[j].Date.Month))
                            countOfChangedTime++;
                        else
                            break;
                    }
                    else if (period == Period.Year)
                    {
                        if (records[i].Date.Year == records[j].Date.Year)
                            countOfChangedTime++;
                        else
                            break;
                    }
                }
                Array.Resize<Record>(ref result, result.Length + 1);
                result[index++] = records[i];
                i = j - 1;
            }
            return result;
        }

        /// <summary>
        /// Получение количества записей за указанный период.
        /// </summary>
        /// <param name="period">Перечисление период.</param>
        /// <returns>Количество записей.</returns>
        public Int32[] GetNumberOfRecordsOnPeriod(Period period)
        {
            Int32[] result = new Int32[0];
            Int32 index = 0;

            for (int i = 0; i < records.Count; i++)
            {
                Int32 countOfChangedTime = 1;
                if (i == records.Count - 1)
                {
                    Array.Resize<Int32>(ref result, result.Length + 1);
                    result[index++] = 1;
                    break;
                }
                int j = i + 1;
                for (; j < records.Count; j++)
                {
                    if (period == Period.Day)
                    {
                        if (records[i].Date.ToShortDateString() == records[j].Date.ToShortDateString())
                            countOfChangedTime++;
                        else
                            break;
                    }
                    else if (period == Period.Month)
                    {
                        if ((records[i].Date.Year == records[j].Date.Year) && (records[i].Date.Month == records[j].Date.Month))
                            countOfChangedTime++;
                        else
                            break;
                    }
                    else if (period == Period.Year)
                    {
                        if (records[i].Date.Year == records[j].Date.Year)
                            countOfChangedTime++;
                        else
                            break;
                    }
                }
                Array.Resize<Int32>(ref result, result.Length + 1);
                result[index++] = countOfChangedTime;
                i = j - 1;
            }
            return result;
        }

        /// <summary>
        /// Получение максимального числа записей за указанный период.
        /// </summary>
        /// <param name="period">Период.</param>
        /// <returns>Максимальное число записей.</returns>
        public Int32 GetMaxRecordsOnPeriodCount(Period period)
        {
            // Рассчитываем, сколько максимум записей было сделано за выбранный промежуток времени.
            // Дикая вещь. Написана в 3:01 ночи, так что лучше тут ничего не трогать.
            Int32 maxRecordsCount = 0;
            for (int i = 0; i < records.Count; i++)
            {
                Int32 countOfChangedTime = 1;
                if (i == records.Count - 1)
                {
                    if (maxRecordsCount < countOfChangedTime)
                        maxRecordsCount = countOfChangedTime;
                    break;
                }
                int j = i + 1;
                for (; j < records.Count; j++)
                {
                    if (period==Period.Day)
                    {
                        if (records[i].Date.ToShortDateString() == records[j].Date.ToShortDateString())
                            countOfChangedTime++;
                        else
                            break;
                    }
                    else if (period == Period.Month)
                    {
                        if ((records[i].Date.Year == records[j].Date.Year) && (records[i].Date.Month == records[j].Date.Month))
                            countOfChangedTime++;
                        else
                            break;
                    }
                    else if (period == Period.Year)
                    {
                        if (records[i].Date.Year == records[j].Date.Year)
                            countOfChangedTime++;
                        else
                            break;
                    }
                }
                if (maxRecordsCount < countOfChangedTime)
                    maxRecordsCount = countOfChangedTime;
                i = j - 1;
            }
            return maxRecordsCount;
        }

        /// <summary>
        /// Получение массива всех записей о событии.
        /// </summary>
        /// <returns>Массив записей.</returns>
        public Record[] GetRecords()
        {
            Record[] temp = new Record[records.Count];
            records.CopyTo(temp);
            return temp;
        }
        /// <summary>
        /// Получение количества записей в событии.
        /// </summary>
        /// <returns>Количество записей в событии.</returns>
        public Int32 GetRecordsCount()
        {
            return records.Count;
        }

        public Record this[Int32 index]
        {
            get
            {
                // Нужна ли эта проверка?
                if (index < 0 || index >= records.Count)
                    throw new IndexOutOfRangeException("Invalid index");
                else
                    return records[index];
            }
        }

        public void Clear()
        {
            records = new List<Record>();
        }

        public object Clone()
        {
            LifeEvent clone = new LifeEvent(ID, Name, Description, Rate);
            foreach (Record record in records)
                clone.AddRecord(record.Date, record.Description);
            return clone;
        }

        public override string ToString()
        {
            return String.Format($"Event: {Name}. Repeatability: {Rate}. Records: {records.Count()}");
        }
    }
}