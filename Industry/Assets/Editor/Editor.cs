using System;
using System.IO;
using System.IO.Hashing;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

using TMPro;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;

[CustomEditor(typeof(Backend))]
public class EditorManagement : Editor
{
    private Backend Backend;

    [Obsolete]
    void OnEnable()
    {
        Backend
        = (Backend)
        target;

        {
            PlayerSettings
            .companyName
            = "KenDawCorp";

            PlayerSettings
            .runInBackground
            = true;

            PlayerSettings
            .captureSingleScreen
            = true;

            PlayerSettings
            .fullScreenMode
            = FullScreenMode
            .ExclusiveFullScreen;

            PlayerSettings
            .graphicsJobs
            = true;

            PlayerSettings
            .graphicsJobMode
            = GraphicsJobMode
            .Native;

            PlayerSettings
            .MTRendering
            = true;

            switch (
            SceneManager
            .GetActiveScene()
            .name)
            {
                case "WebAuth(Module)":
                    {
                        NamedBuildTarget NamedBuildTarget
                        = NamedBuildTarget
                        .FromBuildTargetGroup(
                        EditorUserBuildSettings
                        .selectedBuildTargetGroup);

                        Array Array
                        = new string[]
                        {
                            "Web",
                            "WebAuth"
                        };

                        PlayerSettings
                        .SetScriptingDefineSymbols(
                        NamedBuildTarget,
                        String.Join(
                        ";",
                        (string[])
                        Array));
                    }
                    break;
                case "WebAccessibility(Module)":
                    {
                        NamedBuildTarget NamedBuildTarget
                        = NamedBuildTarget
                        .FromBuildTargetGroup(
                        EditorUserBuildSettings
                        .selectedBuildTargetGroup);

                        Array Array
                        = new string[]
                        {
                            "Web",
                            "WebAccessibility"
                        };

                        PlayerSettings
                        .SetScriptingDefineSymbols(
                        NamedBuildTarget,
                        String.Join(
                        ";",
                        (string[])
                        Array));
                    }
                    break;
                case "WebLaborAssignment(Module)":
                    {
                        NamedBuildTarget NamedBuildTarget
                        = NamedBuildTarget
                        .FromBuildTargetGroup(
                        EditorUserBuildSettings
                        .selectedBuildTargetGroup);

                        Array Array
                        = new string[]
                        {
                            "Web",
                            "WebLaborAssignment"
                        };

                        PlayerSettings
                        .SetScriptingDefineSymbols(
                        NamedBuildTarget,
                        String.Join(
                        ";",
                        (string[])
                        Array));
                    }
                    break;
                case "WebDwelling(Module)":
                    {
                        NamedBuildTarget NamedBuildTarget
                        = NamedBuildTarget
                        .FromBuildTargetGroup(
                        EditorUserBuildSettings
                        .selectedBuildTargetGroup);

                        Array Array
                        = new string[]
                        {
                            "Web",
                            "WebDwelling"
                        };

                        PlayerSettings
                        .SetScriptingDefineSymbols(
                        NamedBuildTarget,
                        String.Join(
                        ";",
                        (string[])
                        Array));
                    }
                    break;
                case "WebFunction(Module)":
                    {
                        NamedBuildTarget NamedBuildTarget
                        = NamedBuildTarget
                        .FromBuildTargetGroup(
                        EditorUserBuildSettings
                        .selectedBuildTargetGroup);

                        Array Array
                        = new string[]
                        {
                            "Web",
                            "WebFunction"
                        };

                        PlayerSettings
                        .SetScriptingDefineSymbols(
                        NamedBuildTarget,
                        String.Join(
                        ";",
                        (string[])
                        Array));
                    }
                    break;
                case "Firmware(Module)":
                    {
                        NamedBuildTarget NamedBuildTarget
                        = NamedBuildTarget
                        .FromBuildTargetGroup(
                        EditorUserBuildSettings
                        .selectedBuildTargetGroup);

                        PlayerSettings
                        .SetScriptingDefineSymbols(
                        NamedBuildTarget,
                        ";Firmware");
                    }
                    break;
                case "OSes(Module)":
                    {
                        NamedBuildTarget NamedBuildTarget
                        = NamedBuildTarget
                        .FromBuildTargetGroup(
                        EditorUserBuildSettings
                        .selectedBuildTargetGroup);

                        PlayerSettings
                        .SetScriptingDefineSymbols(
                        NamedBuildTarget,
                        ";OSes");
                    }
                    break;
            }
        }

        {
            StringBuilder StringBuilder
            = new StringBuilder();

            StringBuilder
            .Append(Application.dataPath);
            StringBuilder
            .Append("/Editor");
            StringBuilder
            .Append("/.Build");

#if OSes || Firmware
            StringBuilder
            .Append("/System");
#endif

            string Path
            = StringBuilder
            .ToString();

            Directory
            .CreateDirectory
            (Path);
        }

        {
            EditorApplication
            .playModeStateChanged -=
            OnPlayModeChanged;

            EditorApplication
            .playModeStateChanged +=
            OnPlayModeChanged;
        }
    }

    private void OnPlayModeChanged(
    PlayModeStateChange
    PlayModeStateChange)
    {
        bool Boolean
        = Backend != null;

        switch (Boolean)
        {
            case true:
                {
                    Array Array
                    = Backend
                    .GetComponentsInChildren
                    <Transform>
                    (true);

                    switch (PlayModeStateChange)
                    {
                        case PlayModeStateChange
                        .ExitingEditMode:
                            {
                                foreach (Transform _Transform
                                in Array)
                                {
                                    string String
                                    = _Transform
                                    .gameObject.name;

                                    Boolean
                                    = String
                                    .StartsWith("Template");

                                    switch (Boolean)
                                    {
                                        case true:
                                            {
                                                _Transform
                                                .gameObject
                                                .SetActive(false);
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                        case PlayModeStateChange
                        .EnteredEditMode:
                            {
                                foreach (Transform _Transform
                                in Array)
                                {
                                    string String
                                    = _Transform
                                    .gameObject.name;

                                    Boolean
                                    = String
                                    .StartsWith("Template");

                                    switch (Boolean)
                                    {
                                        case true:
                                            {
                                                _Transform
                                                .gameObject
                                                .SetActive(true);
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }
                break;
        }
    }

    Dictionary<string, object> OnInspectorGUI_Dictionary
    = new Dictionary<string, object>
    {
        {
            "Template",
            false
        }
    };
    public override async void OnInspectorGUI()
    {
        DrawDefaultInspector();

        {
            GUILayout
            .Space(10f);

            GUILayout
            .Label("Toolschain");

            GUILayout
            .Space(10f);

            {
                GUILayout
                .Label(
                "*Localize");

                StringBuilder StringBuilder
                = new StringBuilder();

                StringBuilder
                .AppendLine
                ("Creates a localization template that allows elements to");
                StringBuilder
                .AppendLine
                ("dynamically switch languages. Path:");

#if Web
                StringBuilder
                .Append("/Editor");
                StringBuilder
                .Append("/.Build");
                StringBuilder
                .Append("/Web");
#if WebAuth
                StringBuilder
                .Append("/auth");
#elif WebAccessibility
                StringBuilder
                .Append("/accessibility");
#elif WebLaborAssignment
                StringBuilder
                .Append("/accessibility");
                StringBuilder
                .AppendLine("/operations");
                StringBuilder
                .Append("/manage");
                StringBuilder
                .Append("/laborassignment");
#elif WebDwelling
                StringBuilder
                .Append("/accessibility");
                StringBuilder
                .AppendLine("/operations");
                StringBuilder
                .Append("/inventory");
                StringBuilder
                .Append("/dwelling");
#elif WebFunction
                StringBuilder
                .Append("/accessibility");
                StringBuilder
                .AppendLine("/operations");
                StringBuilder
                .Append("/inventory");
                StringBuilder
                .Append("/function");
#endif
                StringBuilder
                .Append("/data");
                StringBuilder
                .Append("/lang");
#elif OSes
                StringBuilder
                .Append("/Editor");
                StringBuilder
                .Append("/System");
                StringBuilder
                .Append("/indus");
                StringBuilder
                .Append("/data");
                StringBuilder
                .Append("/Language");
#endif

                string String
                = StringBuilder
                .ToString();

                GUILayout
                .Label(String);

                bool Boolean
                = GUILayout
                .Button
                ("Localize");

                switch (Boolean)
                {
                    case true:
                        {
                            string Path
                            = String.Empty;

                            JObject JObject
                            = new JObject();

                            IEnumerable
                            <TextMeshProUGUI> IEnumerable
                            = Backend
                            .GetComponentsInChildren
                            <TextMeshProUGUI>(true);

                            foreach (
                            TextMeshProUGUI
                            TextMeshProUGUI
                            in IEnumerable)
                            {
                                String
                                = TextMeshProUGUI
                                .text;

                                Boolean
                                = new string[]
                                {
                                    "Title",
                                    "Label",
                                    "Text"
                                }.Any(
                                x =>
                                x ==
                                String)
                                == false

                                && String
                                .Any(
                                x =>
                                char
                                .IsLetter(x))

                                && String
                                .Length > 0;

                                switch (Boolean)
                                {
                                    case true:
                                        {
                                            byte[] Bytes
                                            = Encoding.
                                            UTF8.GetBytes(String);

                                            Bytes
                                            = XxHash64
                                            .Hash(Bytes);

                                            string Index
                                            = Convert
                                            .ToBase64String(Bytes);

                                            Boolean
                                            = JObject
                                            .ContainsKey(Index)
                                            == false;

                                            switch (Boolean)
                                            {
                                                case true:
                                                    {
                                                        JObject
                                                        .Add(
                                                        Index,
                                                        String);

                                                        Debug.Log(
                                                        "Localize: Compute-Hash:"
                                                        + Environment.NewLine
                                                        + Index);
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }

                                await
                                UniTask
                                .Yield();
                            }

                            {
                                StringBuilder
                                = new StringBuilder();
#if Web
                                StringBuilder
                                .Append(Application.dataPath);
                                StringBuilder
                                .Append("/Editor");
                                StringBuilder
                                .Append("/.Build");
                                StringBuilder
                                .Append("/Web");
#if WebAuth
                                StringBuilder
                                .Append("/auth");
#elif WebAccessibility
                                StringBuilder
                                .Append("/accessibility");
#elif WebLaborAssignment
                                StringBuilder
                                .Append("/accessibility");
                                StringBuilder
                                .Append("/operations");
                                StringBuilder
                                .Append("/manage");
                                StringBuilder
                                .Append("/laborassignment");
#elif WebDwelling
                                StringBuilder
                                .Append("/accessibility");
                                StringBuilder
                                .Append("/operations");
                                StringBuilder
                                .Append("/inventory");
                                StringBuilder
                                .Append("/dwelling");
#elif WebFunction
                                StringBuilder
                                .Append("/accessibility");
                                StringBuilder
                                .Append("/operations");
                                StringBuilder
                                .Append("/inventory");
                                StringBuilder
                                .Append("/function");
#endif
                                StringBuilder
                                .Append("/data");
                                StringBuilder
                                .Append("/lang");
                                StringBuilder
                                .Append("/.template");
#elif OSes
                                StringBuilder
                                .Append(Application.dataPath);
                                StringBuilder
                                .Append("/Editor");
                                StringBuilder
                                .Append("/.Build");
                                StringBuilder
                                .Append("/System");
                                StringBuilder
                                .Append("/indus");
                                StringBuilder
                                .Append("/data");
                                StringBuilder
                                .Append("/Language");
                                StringBuilder
                                .Append("/.Template");
#endif

                                Path
                                = StringBuilder
                                .ToString();

                                String
                                = JObject
                                .ToString(
                                Newtonsoft
                                .Json.Formatting
                                .Indented
                                );

                                await File
                                .WriteAllTextAsync(
                                Path,
                                String);
                            }

                            {
                                StringBuilder
                                = new StringBuilder();

                                StringBuilder
                                .AppendLine("Localize Complete:");
                                StringBuilder
                                .AppendLine(Path);

                                String
                                = StringBuilder
                                .ToString();

                                Debug
                                .Log(String);
                            }
                        }
                        break;
                }
            }

            GUILayout
            .Space(10f);

            {
                GUILayout
                .Label(
                "*Template");

                string String
                = "Manually toggle prefab template visibility in the viewer.";

                GUILayout
                .Label(String);

                String
                = "Hide";

                bool Boolean
                = (bool)
                OnInspectorGUI_Dictionary
                ["Template"];

                switch (Boolean)
                {
                    case true:
                        {
                            String
                            = "Show";
                        }
                        break;
                }

                Boolean
                = GUILayout
                .Button
                (String);

                switch (Boolean)
                {
                    case true:
                        {
                            {
                                IEnumerable
                                <Transform> IEnumerable
                                = Backend
                                .GetComponentsInChildren
                                <Transform>(true);

                                foreach (Transform Transform
                                in IEnumerable)
                                {
                                    String
                                    = Transform
                                    .gameObject
                                    .name;

                                    Boolean
                                    = String
                                    .StartsWith
                                    ("Template");

                                    switch (Boolean)
                                    {
                                        case true:
                                            {
                                                Boolean
                                                = (bool)
                                                OnInspectorGUI_Dictionary
                                                ["Template"];

                                                Transform
                                                .gameObject
                                                .SetActive
                                                (Boolean);
                                            }
                                            break;
                                    }
                                }
                            }

                            {
                                Boolean
                                = (bool)
                                OnInspectorGUI_Dictionary
                                ["Template"];

                                OnInspectorGUI_Dictionary
                                ["Template"]
                                = !Boolean;

                                Debug
                                .Log("Manually Template Visibility Complete");
                            }
                        }
                        break;
                }
            }

            GUILayout
            .Space(10f);

#if WebDwelling

            {
                GUILayout
                .Label(
                "*Visualization");

                StringBuilder StringBuilder
                = new StringBuilder();

                StringBuilder
                .AppendLine
                ("Save or Load Architectural Data for 3D Visualization");

                StringBuilder
                .AppendLine("or Mapped Layouts.");

                string String
                = StringBuilder
                .ToString();

                GUILayout
                .Label(String);

                {
                    bool Boolean
                    = GUILayout
                    .Button
                    ("Save");

                    switch (Boolean)
                    {
                        case true:
                            {
                                string Path
                                = EditorUtility
                                .SaveFilePanel
                                ("Visualization",
                                String
                                .Empty,
                                "Visualization",
                                String
                                .Empty);

                                JArray JArray
                                = new JArray();

                                Transform Transform
                                = Backend
                                .Environment
                                .transform.Find
                                ("World[Object]")
                                .transform;

                                foreach (Transform _Transform
                                in Transform)
                                {
                                    String
                                    = _Transform
                                    .gameObject
                                    .name;

                                    Boolean
                                    = String
                                    .StartsWith
                                    ("Template")
                                    == false;

                                    switch (Boolean)
                                    {
                                        case true:
                                            {
                                                JObject JObject
                                                = new JObject();

                                                {
                                                    Vector3 Vector3
                                                    = _Transform
                                                    .position;

                                                    float X
                                                    = Vector3
                                                    .x;

                                                    float Y
                                                    = Vector3
                                                    .y;

                                                    float Z
                                                    = Vector3
                                                    .z;

                                                    JObject
                                                    .Add
                                                    ("Position",
                                                    new JObject
                                                    {
                                                        ["X"]
                                                        = X,

                                                        ["Y"]
                                                        = Y,

                                                        ["Z"]
                                                        = Z
                                                    });
                                                }

                                                {
                                                    Quaternion Quaternion
                                                    = _Transform
                                                    .rotation;

                                                    float X
                                                    = Quaternion
                                                    .x;

                                                    float Y
                                                    = Quaternion
                                                    .y;

                                                    float Z
                                                    = Quaternion
                                                    .z;

                                                    JObject
                                                    .Add
                                                    ("Rotation",
                                                    new JObject
                                                    {
                                                        ["X"]
                                                        = X,

                                                        ["Y"]
                                                        = Y,

                                                        ["Z"]
                                                        = Z
                                                    });
                                                }

                                                {
                                                    Vector3 Vector3
                                                    = _Transform
                                                    .localScale;

                                                    float X
                                                    = Vector3
                                                    .x;

                                                    float Y
                                                    = Vector3
                                                    .y;

                                                    float Z
                                                    = Vector3
                                                    .z;

                                                    JObject
                                                    .Add
                                                    ("Scale",
                                                    new JObject
                                                    {
                                                        ["X"]
                                                        = X,

                                                        ["Y"]
                                                        = Y,

                                                        ["Z"]
                                                        = Z
                                                    });
                                                }

                                                {
                                                    String
                                                    = _Transform
                                                    .gameObject
                                                    .name;

                                                    int Index
                                                    = String
                                                    .IndexOf
                                                    ("[");

                                                    String
                                                    = String
                                                    .Substring(
                                                    0, Index);

                                                    JObject
                                                    .Add
                                                    ("Addressable",
                                                    String);
                                                }

                                                {
                                                    String
                                                    = _Transform
                                                    .Find("Label[Text]")
                                                    .GetComponent
                                                    <TextMeshPro>()
                                                    .text;

                                                    JObject
                                                    .Add
                                                    ("Label",
                                                    String);
                                                }

                                                JArray
                                                .Add
                                                (JObject);
                                            }
                                            break;
                                    }
                                }

                                {
                                    {
                                        String
                                        = JArray
                                        .ToString
                                        (Newtonsoft
                                        .Json
                                        .Formatting
                                        .Indented);

                                        await File
                                        .WriteAllTextAsync
                                        (Path, String);
                                    }

                                    {
                                        StringBuilder
                                        = new StringBuilder();

                                        StringBuilder
                                        .Append
                                        ("Visualization Data Saved at:");
                                        StringBuilder
                                        .Append(
                                        Environment
                                        .NewLine);
                                        StringBuilder
                                        .Append(Path);

                                        String
                                        = StringBuilder
                                        .ToString();

                                        Debug
                                        .Log(String);
                                    }
                                }
                            }
                            break;
                    }
                }

                GUILayout
                .Space(10f);

                {
                    bool Boolean
                    = GUILayout
                    .Button
                    ("Load");

                    switch (Boolean)
                    {
                        case true:
                            {
                                bool isLocked
                                = false;

                                Dictionary<string, GameObject> Dictionary
                                = new Dictionary
                                <string, GameObject>();

                                Transform Transform
                                = Backend
                                .Environment
                                .transform.Find
                                ("World[Object]")
                                .transform;

                                string Path
                                = EditorUtility
                                .OpenFilePanel
                                ("Visualization",
                                String
                                .Empty,
                                String
                                .Empty);

                                String
                                = await File
                                .ReadAllTextAsync
                                (Path);

                                JArray JArray
                                = JArray
                                .Parse
                                (String);

                                int Length
                                = JArray
                                .Count;

                                for (int i = 0; i < Length; i++)
                                {
                                    JToken JToken
                                    = JArray
                                    .ElementAt(i);

                                    StringBuilder
                                    .Append(i);
                                    StringBuilder
                                    .Append
                                    ("[Model]");

                                    String
                                    = StringBuilder
                                    .ToString();

                                    Boolean
                                    = Transform
                                    .Find(String)
                                    == null;

                                    switch (Boolean)
                                    {
                                        case true:
                                            {
                                                GameObject GameObject
                                                = null;

                                                {
                                                    StringBuilder
                                                    = new StringBuilder();

                                                    StringBuilder
                                                    .Append("Accessibility");
                                                    StringBuilder
                                                    .Append("/Operations");
                                                    StringBuilder
                                                    .Append("/Inventory");
                                                    StringBuilder
                                                    .Append("/Dwelling/");

                                                    String
                                                    = (string)
                                                    JToken
                                                    ["Addressable"];

                                                    StringBuilder
                                                    .Append(String);

                                                    Path
                                                    = StringBuilder
                                                    .ToString();

                                                    Boolean
                                                    = Dictionary
                                                    .ContainsKey
                                                    (Path)
                                                    == false;

                                                    switch (Boolean)
                                                    {
                                                        case true:
                                                            {
                                                                {
                                                                    GameObject
                                                                    = Instantiate(
                                                                    Transform.Find
                                                                    ("Template[Model]")
                                                                    .gameObject,
                                                                    Transform);
                                                                }

                                                                {
                                                                    bool _Boolean
                                                                    = isLocked
                                                                    == false

                                                                    && Dictionary
                                                                    .ContainsKey
                                                                    (Path)
                                                                    == false;

                                                                    switch (_Boolean)
                                                                    {
                                                                        case true:
                                                                            {
                                                                                isLocked
                                                                                = true;

                                                                                AsyncOperationHandle
                                                                                <GameObject>
                                                                                AsyncOperationHandle
                                                                                = Addressables
                                                                                .LoadAssetAsync
                                                                                <GameObject>(Path);

                                                                                AsyncOperationHandle
                                                                                .Completed
                                                                                += async (Handle)
                                                                                =>
                                                                                {
                                                                                    Boolean
                                                                                    = Handle
                                                                                    .Status
                                                                                    == AsyncOperationStatus
                                                                                    .Succeeded;

                                                                                    switch (Boolean)
                                                                                    {
                                                                                        case true:
                                                                                            {
                                                                                                GameObject GameObject
                                                                                                = Handle
                                                                                                .Result;

                                                                                                Dictionary
                                                                                                .Add(
                                                                                                Path,
                                                                                                GameObject);

                                                                                                i = -1;
                                                                                            }
                                                                                            break;
                                                                                    }

                                                                                    isLocked
                                                                                    = false;
                                                                                };
                                                                            }
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                        case false:
                                                            {
                                                                {
                                                                    StringBuilder
                                                                    = new StringBuilder();

                                                                    StringBuilder
                                                                    .Append("*");
                                                                    StringBuilder
                                                                    .Append(i);
                                                                    StringBuilder
                                                                    .Append("[Model]");

                                                                    String
                                                                    = StringBuilder
                                                                    .ToString();

                                                                    bool _Boolean
                                                                    = Transform.Find
                                                                    (String)
                                                                    != null;

                                                                    switch (_Boolean)
                                                                    {
                                                                        case true:
                                                                            {
                                                                                GameObject
                                                                                = Transform.Find
                                                                                (String)
                                                                                .gameObject;

                                                                                DestroyImmediate
                                                                                (GameObject);
                                                                            }
                                                                            break;
                                                                    }
                                                                }

                                                                {
                                                                    GameObject
                                                                    = Dictionary
                                                                    [Path];

                                                                    GameObject
                                                                    = Instantiate(
                                                                    GameObject,
                                                                    Transform);
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }

                                                {
                                                    StringBuilder
                                                    = new StringBuilder();

                                                    switch (Boolean)
                                                    {
                                                        case true:
                                                            {
                                                                StringBuilder
                                                                .Append("*");
                                                            }
                                                            break;
                                                    }

                                                    StringBuilder
                                                    .Append(i);
                                                    StringBuilder
                                                    .Append("[Model]");

                                                    String
                                                    = StringBuilder
                                                    .ToString();

                                                    GameObject
                                                    .name
                                                    = String;
                                                }

                                                {
                                                    Transform _Transform
                                                    = GameObject
                                                    .transform;

                                                    {
                                                        Vector3 Vector3
                                                        = new Vector3();

                                                        Vector3
                                                        .x
                                                        = (float)
                                                        JToken
                                                        ["Position"]
                                                        ["X"];

                                                        Vector3
                                                        .y
                                                        = (float)
                                                        JToken
                                                        ["Position"]
                                                        ["Y"];

                                                        Vector3
                                                        .z
                                                        = (float)
                                                        JToken
                                                        ["Position"]
                                                        ["Z"];

                                                        _Transform
                                                        .position
                                                        = Vector3;
                                                    }

                                                    {
                                                        Quaternion Quaternion
                                                        = new Quaternion();

                                                        Quaternion
                                                        .x
                                                        = (float)
                                                        JToken
                                                        ["Rotation"]
                                                        ["X"];

                                                        Quaternion
                                                        .y
                                                        = (float)
                                                        JToken
                                                        ["Rotation"]
                                                        ["Y"];

                                                        Quaternion
                                                        .z
                                                        = (float)
                                                        JToken
                                                        ["Rotation"]
                                                        ["Z"];

                                                        _Transform
                                                        .rotation
                                                        = Quaternion;
                                                    }

                                                    {
                                                        Vector3 Vector3
                                                        = new Vector3();

                                                        Vector3
                                                        .x
                                                        = (float)
                                                        JToken
                                                        ["Scale"]
                                                        ["X"];

                                                        Vector3
                                                        .y
                                                        = (float)
                                                        JToken
                                                        ["Scale"]
                                                        ["Y"];

                                                        Vector3
                                                        .z
                                                        = (float)
                                                        JToken
                                                        ["Scale"]
                                                        ["Z"];

                                                        _Transform
                                                        .localScale
                                                        = Vector3;
                                                    }
                                                }

                                                {
                                                    GameObject
                                                    .transform.Find
                                                    ("Label[Text]")
                                                    .GetComponent
                                                    <TextMeshPro>()
                                                    .text
                                                    = (string)
                                                    JToken
                                                    ["Label"];
                                                }

                                                GameObject
                                                .transform
                                                .SetAsLastSibling();

                                                GameObject
                                                .SetActive
                                                (true);

                                                await UniTask
                                                .NextFrame();
                                            }
                                            break;
                                    }
                                }

                                {
                                    StringBuilder
                                    = new StringBuilder();

                                    StringBuilder
                                    .Append
                                    ("Visualization Data Loaded.");
                                    StringBuilder
                                    .Append(
                                    Environment
                                    .NewLine);
                                    StringBuilder
                                    .Append(Path);

                                    String
                                    = StringBuilder
                                    .ToString();

                                    Debug
                                    .Log(String);
                                }
                            }
                            break;
                    }
                }

                GUILayout
                .Space(10f);

                {
                    bool Boolean
                    = GUILayout
                    .Button
                    ("Clear");

                    switch (Boolean)
                    {
                        case true:
                            {
                                {
                                    Transform Transform
                                    = Backend
                                    .Environment
                                    .transform.Find
                                    ("World[Object]")
                                    .transform;

                                    int Length
                                    = Transform
                                    .childCount;

                                    for (int i = 0; i < Length; i++)
                                    {
                                        GameObject GameObject
                                        = Transform
                                        .GetChild(i)
                                        .gameObject;

                                        String
                                        = GameObject
                                        .name;

                                        Boolean
                                        = String
                                        .StartsWith
                                        ("Template")
                                        == false;

                                        switch (Boolean)
                                        {
                                            case true:
                                                {
                                                    DestroyImmediate
                                                    (GameObject);
                                                    i--;
                                                }
                                                break;
                                        }
                                    }

                                    {
                                        Debug
                                        .Log("Visualization Data Cleared.");
                                    }
                                }
                            }
                            break;
                    }
                }
            }

#endif

            GUILayout
            .Space(10f);

            {
                GUILayout
                .Label(
                "*Data");

                StringBuilder StringBuilder
                = new StringBuilder();

                StringBuilder
                .AppendLine("Removes all keys and values from the preferences.");

                StringBuilder
                .Append("Use with caution.");

                string String
                = StringBuilder
                .ToString();

                GUILayout
                .Label(String);

                bool Boolean
                = GUILayout
                .Button
                ("Delete All");

                switch (Boolean)
                {
                    case true:
                        {
                            {
                                PlayerPrefs
                                .DeleteAll();
                            }

                            {
                                Debug
                                .Log("Data Deletion Complete");
                            }
                        }
                        break;
                }
            }
        }
    }

    public int callbackOrder
    => 0;

    public void OnPreprocessBuild(
    BuildReport
    BuildReport)
    {
        {
            Array Array
            = Backend
            .GetComponentsInChildren<Transform>(true);

            foreach (Transform _Transform
            in Array)
            {
                string String
                = _Transform
                .gameObject.name;

                bool Boolean
                = String
                .StartsWith("Template");

                switch (Boolean)
                {
                    case true:
                        {
                            _Transform
                            .gameObject
                            .SetActive(false);
                        }
                        break;
                }
            }
        }

        {
            bool Boolean
            = EditorSceneManager
            .SaveCurrentModifiedScenesIfUserWantsTo();

            switch (Boolean)
            {
                case true:
                    {
                        Debug
                        .Log("All modified scenes saved.");
                    }
                    break;
                case false:
                    {
                        Debug
                        .LogWarning("⚠ Build cancelled by user (unsaved scenes).");

                        throw new
                        BuildFailedException("Build cancelled due to unsaved scenes.");
                    }
            }

            AssetDatabase
            .SaveAssets();
        }
    }
}

// Copyright © 2025 KenDaw.