using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace RYL__record_your_life_
{
    [Serializable]
    public sealed class RYL : IEnumerable, IEnumerator
    {
        private Int32 index; // Для реализации интерфейса.
        private List<LifeEvent> lifeEvents;

        public RYL()
        {
            index = -1;
            lifeEvents = new List<LifeEvent>();
        }

        /// <summary>
        /// Добавление события. Примечание: имена должны быть уникальными.
        /// </summary>
        /// <param name="name">Уникальное имя события.</param>
        /// <param name="description">Описание.</param>
        /// <param name="rate">Повторяемость.</param>
        public void AddEvent(String name, String description, Repeatability rate)
        {
            if (name == null || description == null)
                throw new ArgumentNullException("Null argument.");
            if (name.Length < 1)
                throw new ArgumentException("Name length must be more than 0.");
            if (!IsNameUnique(name))
                throw new NotUniqueNameException("Имя события должно быть уникальным.");

            lifeEvents.Add(new LifeEvent(lifeEvents.Count, name, description, rate));
        }

        /// <summary>
        /// Удаление события.
        /// </summary>
        /// <param name="ID">ID события.</param>
        public void DeleteEvent(Int32 ID)
        {
            if (ID < 0 || ID >= lifeEvents.Count)
                throw new ArgumentOutOfRangeException("Invalid ID.");

            if (ID != lifeEvents.Count - 1)
            {
                for (Int32 i = ID + 1; i < lifeEvents.Count; i++)
                    lifeEvents[i].ChangeID(lifeEvents[i].ID-1);
            }
            lifeEvents.RemoveAt(ID);
        }

        /// <summary>
        /// Добавление записи о событии.
        /// </summary>
        /// <param name="date">Дата.</param>
        /// <param name="description">Описание.</param>
        /// <param name="eventID">Уникальный ID события.</param>
        public void AddRecord(DateTime date, String description, Int32 eventID)
        {
            if (description == null)
                throw new ArgumentNullException("Null argument.");
            if (eventID < 0 || eventID >= lifeEvents.Count)
                throw new ArgumentOutOfRangeException("Invalid eventID.");
            lifeEvents[eventID].AddRecord(date, description);
        }

        /// <summary>
        /// Меняет местами события.
        /// </summary>
        /// <param name="ID1">ID первого события.</param>
        /// <param name="ID2">ID второго события.</param>
        private void SwapEvents(Int32 ID1, Int32 ID2)
        {
            if (ID1 < 0 || ID2 < 0 || ID1 >= lifeEvents.Count || ID2 >= lifeEvents.Count)
                throw new ArgumentOutOfRangeException("Invalid argument(s)");
            if (ID1 == ID2)
                return;

            LifeEvent temp = (LifeEvent)lifeEvents[ID1].Clone();
            lifeEvents[ID1] = lifeEvents[ID2];
            lifeEvents[ID2] = temp;

            lifeEvents[ID1].ChangeID(ID1);
            lifeEvents[ID2].ChangeID(ID2);

        }

        /// <summary>
        /// Сдвиг события в массиве вверх.
        /// </summary>
        /// <param name="eventID">ID события</param>
        /// <returns>Возвращает true, если сдвиг прошел успешно, иначе false.</returns>
        public bool TryMoveEventUp(Int32 eventID)
        {
            if (eventID < 0 || eventID >= lifeEvents.Count)
                throw new ArgumentOutOfRangeException("Invalid Argument.");
            if (eventID == 0)
                return false;

            SwapEvents(eventID, eventID - 1);

            return true;
        }
        /// <summary>
        /// Сдвиг события в массиве вниз.
        /// </summary>
        /// <param name="eventID">ID события</param>
        /// <returns>Возвращает true, если сдвиг прошел успешно, иначе false.</returns>
        public bool TryMoveEventDown(Int32 eventID)
        {
            if (eventID < 0 || eventID >= lifeEvents.Count)
                throw new ArgumentOutOfRangeException("Invalid Argument.");
            if (eventID == lifeEvents.Count - 1)
                return false;
        
            SwapEvents(eventID, eventID + 1);

            return true;
        }

        /// <summary>
        /// Поиск событий по имени.
        /// </summary>
        /// <param name="template">Шаблон дя поиска.</param>
        /// <returns>Массив событий, имена которых совпадают с заданным шаблоном.</returns>
        public LifeEvent[] SearchInEvents(String template)
        {
            if (template == null) throw new ArgumentNullException("Null argument.");
            if (lifeEvents.Count < 1) return null;

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

            var searchRequest = lifeEvents.
                Where(lifeEvent => Regex.Match(lifeEvent.Name, pattern, RegexOptions.IgnoreCase).Length > 0).
                Select(lifeEvent => lifeEvent);
            if (searchRequest.Count() > 0)
            {
                LifeEvent[] result = new LifeEvent[searchRequest.Count()];
                int i = 0;
                foreach (var lifeEvent in searchRequest)
                    result[i++] = lifeEvent;
                return result;
            }
            else
                return null;
        }

        /// <summary>
        /// Проверка уникальности имени события.
        /// </summary>
        /// <param name="eventName">Имя события.</param>
        /// <returns>Возвращает true, если имя еще не использовано, иначе false.</returns>
        public bool IsNameUnique(String eventName)
        {
            foreach (LifeEvent lifeEvent in lifeEvents)
            {
                if (eventName == lifeEvent.Name)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Получение события
        /// </summary>
        /// <param name="ID">ID события.</param>
        /// <returns>Событие.</returns>
        public LifeEvent GetEventByID(Int32 ID)
        {
            if (ID < 0 || ID >= lifeEvents.Count) return null;
            return (LifeEvent)lifeEvents[ID].Clone();
        }
        /// <summary>
        /// Получение имени события по его ID.
        /// </summary>
        /// <param name="ID">ID события.</param>
        /// <returns>Имя события.</returns>
        public String GetEventNameByID(Int32 ID)
        {
            if (ID >= lifeEvents.Count || ID < 0) return null;
            return lifeEvents[ID].Name;
        }

        /// <summary>
        /// Получение ID события по имени.
        /// </summary>
        /// <param name="name">Имя события.</param>
        /// <returns>ID</returns>
        public Int32 GetEventIDByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("Null argument.");

            foreach (LifeEvent lifeEvent in lifeEvents)
            {
                if (lifeEvent.Name == name) return lifeEvent.ID;
            }
            return -1;
        }

        /// <summary>
        /// Получение массива всех событий.
        /// </summary>
        /// <returns>Массив событий.</returns>
        public LifeEvent[] GetEvents()
        {
            LifeEvent[] temp = new LifeEvent[lifeEvents.Count];
            lifeEvents.CopyTo(temp);
            return temp;
        }

        /// <summary>
        /// Получение количества событий.
        /// </summary>
        /// <returns>Количество событий.</returns>
        public Int32 GetEventsCount()
        {
            return lifeEvents.Count;
        }

        /// <summary>
        /// Получение массива имен событий.
        /// </summary>
        /// <returns>Массив имен событий.</returns>
        public String[] GetEventsNames()
        {
            String[] temp = new String[lifeEvents.Count];
            for (int i = 0; i < lifeEvents.Count; i++)
                temp[i] = lifeEvents[i].Name;
            return temp;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }
        public bool MoveNext()
        {
            if (index == lifeEvents.Count - 1)
            {
                Reset();
                return false;
            }
            index++;
            return true;
        }
        public void Reset()
        {
            index = -1;
        }
        public object Current
        {
            get
            {
                return lifeEvents[index];
            }
        }

        public LifeEvent this[Int32 index]
        {
            get
            {
                // ?
                if (index < 0 || index >= lifeEvents.Count)
                    throw new IndexOutOfRangeException("Invalid index.");
                return (LifeEvent)lifeEvents[index].Clone();
            }
        }

        public void Clear()
        {
            lifeEvents = new List<LifeEvent>();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class NotUniqueNameException : Exception
    {
        public NotUniqueNameException() { }

        public NotUniqueNameException(String message) : base(message) { }

        public NotUniqueNameException(String message, Exception innerException) : base(message, innerException) { }

        protected NotUniqueNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}