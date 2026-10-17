using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEngine;
using UnityEngine.Networking;

using Cysharp.Threading.Tasks;
using System.Diagnostics;

public class CManage : MonoBehaviour
{
    public class Http
    {
        public int ID
        { get; set; } = 0;
        public string Url
        { get; set; }
        public string Form
        { get; set; }
        = String.Empty;
        public Dictionary<string, string> HttpHeader
        { get; set; }
        = new Dictionary<string, string>();
        public Method HttpMethod
        { get; internal set; }
        public Cache HttpCache
        { get; set; }
        = Http.Cache.Auto;
        public Priority HttpPriority
        { get; set; }
        = Http.Priority.Moderate;

        private DateTime DateTime
        { get; set; } = DateTime.UtcNow;
        private bool isDone
        { get; set; } = false;
        private byte[] Data
        { get; set; }

        public Http()
        { }

        public enum Method
        {
            GET,
            POST
        }

        public enum Cache
        {
            Never,
            Auto
        }

        public enum Priority
        {
            Low,
            Moderate,
            Highest
        }

        public async UniTask<byte[]> GetAsync(
        )
        {
            Dictionary<int, Http> Dictionary
            = Manage_Dictionary;

            System.Random Random
            = new System.Random();

            int Index
            = 0;

            while (true)
            {
                Index
                = Random
                .Next(0, 9999);

                bool Boolean
                = Dictionary
                .ContainsKey
                (Index)
                == false;

                if (Boolean)
                { break; }

                await
                UniTask
                .Yield();
            }

            Http Http
            = new Http();

            {
                Http.ID
                = ID;

                Http.Url
                = Url;

                Http.HttpHeader
                = HttpHeader;

                Http.HttpMethod
                = Method.GET;

                Http.HttpCache
                = HttpCache;

                Http.HttpPriority
                = HttpPriority;
            }

            Dictionary
            .Add(
            Index,
            Http);

            while (Http.isDone
            == false)
            {
                await
                UniTask
                .NextFrame();
            }

            byte[] Bytes
            = Http.Data;

            return
            Bytes;
        }

        public async UniTask<byte[]> PostAsync(
        )
        {
            Dictionary<int, Http> Dictionary
            = Manage_Dictionary;

            System.Random Random
            = new System.Random();

            int Index
            = 0;

            while (true)
            {
                Index
                = Random
                .Next(0, 9999);

                bool Boolean
                = Dictionary
                .ContainsKey
                (Index)
                == false;

                if (Boolean)
                { break; }

                await
                UniTask
                .Yield();
            }

            Http Http
            = new Http();

            {
                Http.ID
                = ID;

                Http.Url
                = Url;

                Http.HttpHeader
                = HttpHeader;

                Http.Form
                = Form;

                Http.HttpMethod
                = Method.POST;

                Http.HttpCache
                = HttpCache;

                Http.HttpPriority
                = HttpPriority;
            }

            Dictionary
            .Add(
            Index,
            Http);

            while (Http.isDone
            == false)
            {
                await
                UniTask
                .NextFrame();
            }

            byte[] Bytes
            = Http.Data;

            return
            Bytes;
        }

        public static long CacheCurrentSize
        { get; internal set; } = 0;
        public static long CacheMaximumSize
        { get; set; } = 2147483648;
        public static Http Current
        { get; internal set; }

        private static Dictionary<int, Http> Manage_Dictionary
        = new Dictionary<int, Http>();
        private static Dictionary<string, byte[]> Manage_Cache_Dictionary
        = new Dictionary<string, byte[]>();

        internal static async UniTask Manage()
        {
            while (true)
            {
                {
                    int Length
                    = Manage_Dictionary
                    .Count();

                    bool Boolean
                    = Length
                    > 0;

                    switch (Boolean)
                    {
                        case true:
                            {
                                Http Http
                                = await
                                Queue();

                                await
                                Worker
                                (Http);
                            }
                            break;
                    }
                }

                {
                    bool Boolean
                    = CacheCurrentSize
                    >= CacheMaximumSize;

                    switch (Boolean)
                    {
                        case true:
                            {
                                Dictionary<string, byte[]> Dictionary
                                = Manage_Cache_Dictionary;

                                for (int i = 0; i < Dictionary.Count(); i++)
                                {
                                    KeyValuePair<string, byte[]> KeyValuePair
                                    = Dictionary
                                    .ElementAt(i);

                                    {
                                        byte[] Bytes
                                        = KeyValuePair
                                        .Value;

                                        int Length
                                        = Bytes
                                        .Length;

                                        CacheCurrentSize
                                        -= Length;
                                    }

                                    {
                                        string String
                                        = KeyValuePair
                                        .Key;

                                        Dictionary
                                        .Remove(String);
                                    }

                                    await UniTask
                                    .Yield();
                                }
                            }
                            break;
                    }
                }

                await
                UniTask
                .NextFrame();
            }
        }

        internal static async UniTask<Http> Queue()
        {
            Dictionary<int, Http> Dictionary
            = Manage_Dictionary;

            List<Http> List
            = new List<Http>();

            {
                Array Array
                = new Http.Priority[]
                {
                    Http.Priority.Low,
                    Http.Priority.Moderate,
                    Http.Priority.Highest
                };

                for (int i = Array.Length - 1; i >= 0; i--)
                {
                    Http.Priority HttpPriority
                    = (Http.Priority)
                    Array
                    .GetValue(i);

                    bool Boolean
                    = Dictionary
                    .Values
                    .Any(
                    x =>
                    x.HttpPriority
                    == HttpPriority);

                    if (Boolean)
                    {
                        {
                            int Columns
                            = Dictionary
                            .Count;

                            for (int x = 0; x < Columns; x++)
                            {
                                KeyValuePair<int, Http>
                                KeyValuePair
                                = Dictionary
                                .ElementAt(x);

                                Http Http
                                = KeyValuePair
                                .Value;

                                Boolean
                                = Http
                                .HttpPriority
                                == HttpPriority;

                                switch (Boolean)
                                {
                                    case true:
                                        {
                                            List
                                            .Add(Http);
                                        }
                                        break;
                                }

                                await UniTask
                                .Yield();
                            }
                        }

                        break;
                    }
                }
            }

            {
                IOrderedEnumerable<Http> IOrderedEnumerable
                = List
                .OrderByDescending(
                x =>
                x.DateTime);

                Http Http
                = IOrderedEnumerable
                .FirstOrDefault();

                return
                Http;
            }
        }

        internal static async UniTask Worker(
        Http Http)
        {
            Current
            = Http;

            string String
            = String.Empty;

            byte[] Bytes
            = new byte[0];

            using (MD5 MD5
            = MD5.Create())
            {
                switch (Http.HttpMethod)
                {
                    case Method.GET:
                        {
                            Bytes
                            = Encoding.UTF8
                            .GetBytes(
                            Http.Url);
                        }
                        break;
                    case Method.POST:
                        {
                            Bytes
                            = Encoding.UTF8
                            .GetBytes(
                            Http.Form);
                        }
                        break;
                }

                byte[] Hash
                = MD5
                .ComputeHash(Bytes);

                StringBuilder StringBuilder
                = new StringBuilder();

                foreach (byte Byte in Hash)
                {
                    String
                    = Byte
                    .ToString("x2");

                    StringBuilder
                    .Append(String);
                }

                String
                = StringBuilder
                .ToString();
            }

            bool Boolean
            = Manage_Cache_Dictionary
            .ContainsKey(String)
            == false;

            switch (Boolean)
            {
                case true:
                    {
                        switch (Http.HttpMethod)
                        {
                            case Method.GET:
                                {
                                    using (UnityWebRequest UnityWebRequest
                                    = UnityWebRequest
                                    .Get(
                                    Http.Url))
                                    {
                                        foreach (KeyValuePair<string, string> KeyValuePair
                                        in Http
                                        .HttpHeader)
                                        {
                                            UnityWebRequest
                                            .SetRequestHeader(
                                            KeyValuePair
                                            .Key,
                                            KeyValuePair
                                            .Value);
                                        }

                                        UnityWebRequest
                                        .downloadHandler
                                        = new DownloadHandlerBuffer();

                                        await UnityWebRequest
                                        .SendWebRequest();

                                        Boolean
                                        = UnityWebRequest
                                        .downloadHandler
                                        .data
                                        != null;

                                        switch (Boolean)
                                        {
                                            case true:
                                                {
                                                    Bytes
                                                    = UnityWebRequest
                                                    .downloadHandler
                                                    .data;
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                            case Method.POST:
                                {
                                    using (UnityWebRequest UnityWebRequest
                                    = UnityWebRequest
                                    .PostWwwForm(
                                    Http.Url,
                                    Http.Form))
                                    {
                                        foreach (KeyValuePair<string, string> KeyValuePair
                                        in Http
                                        .HttpHeader)
                                        {
                                            UnityWebRequest
                                            .SetRequestHeader(
                                            KeyValuePair
                                            .Key,
                                            KeyValuePair
                                            .Value);
                                        }

                                        UnityWebRequest
                                        .downloadHandler
                                        = new DownloadHandlerBuffer();

                                        await UnityWebRequest
                                        .SendWebRequest();

                                        Boolean
                                        = UnityWebRequest
                                        .downloadHandler
                                        .data
                                        != null;

                                        switch (Boolean)
                                        {
                                            case true:
                                                {
                                                    Bytes
                                                    = UnityWebRequest
                                                    .downloadHandler
                                                    .data;
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                        }

                        {
                            Http
                            .Data
                            = Bytes;

                            {
                                int Length
                                = Bytes
                                .Length;

                                Boolean
                                = Length
                                > 0;

                                switch (Boolean)
                                {
                                    case true:
                                        {
                                            switch (Http.HttpCache)
                                            {
                                                case Http
                                                .Cache
                                                .Auto:
                                                    {
                                                        Manage_Cache_Dictionary
                                                        .Add(
                                                        String,
                                                        Bytes);

                                                        CacheCurrentSize
                                                        += Length;
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case false:
                    {
                        Bytes
                        = Manage_Cache_Dictionary
                        [String];

                        Http
                        .Data
                        = Bytes;
                    }
                    break;
            }

            {
                Http
                .isDone
                = true;

                IEnumerable<KeyValuePair<int, Http>> IEnumerable
                = Manage_Dictionary
                .Where(
                x =>
                x.Value
                == Http);

                KeyValuePair<int, Http> KeyValuePair
                = IEnumerable
                .FirstOrDefault();

                int Index
                = KeyValuePair
                .Key;

                Manage_Dictionary
                .Remove(Index);
            }

            await UniTask
            .Yield();
        }
    }

    public static async UniTask Runtime()
    {
        {
            _ =
            Http.Manage();
        }
    }
}
