using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace TakEngine.Notation
{
    public class GameRecord
    {
        public Dictionary<string, string> Tags = new Dictionary<string, string>();
        public List<MoveNotation> MoveNotations = new List<MoveNotation>();
        public string ResultCode;

        public static class StandardTags
        {
            /// <summary>
            /// Game size
            /// Expected values: {4,5,6,7,8,4x4,5x5,6x6,etc...}
            /// </summary>
            public const string Size = "Size";
            public const string Result = "Result";
            public const string Site = "Site";
            public const string Player1 = "Player1";
            public const string Player2 = "Player2";
            public const string Event = "Event";
            public const string Date = "Date";
            public const string Time = "Time";
        }

        public int BoardSize
        {
            get
            {
                string s;
                if (!Tags.TryGetValue(StandardTags.Size, out s))
                    throw new ApplicationException("Game size undefined");
                for (int size = 4; size <= 8; size++)
                {
                    string shortVersion = size.ToString();
                    string longVersion = shortVersion + "x" + shortVersion;
                    if (s == shortVersion || s == longVersion)
                        return size;
                }
                throw new ApplicationException("Unsupported game size");
            }
            set
            {
                Tags[StandardTags.Size] = string.Format("{0}", value);
            }
        }

        public DateTime? Date
        {
            get
            {
                string s;
                if (Tags.TryGetValue(StandardTags.Date, out s))
                {
                    DateTime d;
                    if (!DateTime.TryParse(s.Replace(".", "-"), out d))
                        return null;
                    TimeSpan ts;
                    if (Tags.TryGetValue(StandardTags.Time, out s) && TimeSpan.TryParse(s, out ts))
                        d += ts;
                    return d;
                }
                else
                    return null;
            }
            set
            {
                if (value.HasValue)
                {
                    Tags[StandardTags.Date] = value.Value.ToString("yyyy.MM.dd");
                    Tags[StandardTags.Time] = value.Value.ToString("HH:mm:ss");
                }
                else
                {
                    Tags.Remove(StandardTags.Date);
                    Tags.Remove(StandardTags.Time);
                }
            }
        }

        public string Site
        {
            get { return GetStringValue(StandardTags.Site); }
            set { SetStringValue(StandardTags.Site, value); }
        }

        public string Event
        {
            get { return GetStringValue(StandardTags.Event); }
            set { SetStringValue(StandardTags.Event, value); }
        }

        public string Player1
        {
            get { return GetStringValue(StandardTags.Player1); }
            set { SetStringValue(StandardTags.Player1, value); }
        }

        public string Player2
        {
            get { return GetStringValue(StandardTags.Player2); }
            set { SetStringValue(StandardTags.Player2, value); }
        }

        string GetStringValue(string tag)
        {
            string s;
            if (!Tags.TryGetValue(tag, out s))
                s = null;
            return s;
        }

        void SetStringValue(string tag, string s)
        {
            if (string.IsNullOrEmpty(s))
                Tags.Remove(tag);
            else
                Tags[tag] = s;
        }

        public string Result
        {
            get
            {
                string result = null;
                Tags.TryGetValue(StandardTags.Result, out result);
                return result;
            }
            set
            {
                string resultValue = value;
                if (string.IsNullOrEmpty(resultValue))
                    resultValue = "(no result)";
                Tags[StandardTags.Result] = resultValue;
            }
        }

        public void Write(TextWriter writer)
        {
            foreach (var key in Tags.Keys.OrderBy(x => x))
            {
                var tagValue = Tags[key];

                // escape special characters
                string escaped = tagValue.Replace("\\", "\\\\").Replace("\"", "\\\"");
                //writer.WriteLine("[{0,-10} \"{1}\"]", key, escaped);
                writer.WriteLine("[{0} \"{1}\"]", key, escaped);
            }

            int ply = -1;
            foreach (var notation in MoveNotations)
            {
                ply++;
                if (0 == (ply & 1))
                {
                    writer.WriteLine();
                    int turn = (ply >> 1) + 1;
                    writer.Write("{0}. ", turn);
                }

                //writer.Write("{0,-10}", notation.Text);
                writer.Write("{0}", notation.Text);
                if (0 == (ply & 1))
                    writer.Write(" ");
            }
            if (!string.IsNullOrEmpty(ResultCode))
            {
                writer.Write(" ");
                writer.WriteLine(ResultCode);
            }
            else
                writer.Write(" *");
            writer.WriteLine();
        }
    }
}
