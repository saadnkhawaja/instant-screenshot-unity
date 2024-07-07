using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class InstantScreenshot : EditorWindow
{
    private int resWidth = Screen.width * 4;
    private int resHeight = Screen.height * 4;
    private Camera screenshotCamera;
    private int scale = 1;
    private string savePath = "";
    private bool showPreview = true;
    private RenderTexture renderTexture;
    private bool isTransparent = false;
    private bool takeHighResShot = false;
    private string lastScreenshot = "";

    [MenuItem("Tools/Saad Khawaja/Instant High-res Screenshot")]
    public static void ShowWindow()
    {
        EditorWindow editorWindow = GetWindow<InstantScreenshot>();
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Screenshot");
    }

    private Vector2 scrollPosition;
    private void OnGUI()
    {
        // Add padding to the window
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        GUILayout.Space(10);

        // Create a scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));

        // Create a GUIStyle for the label
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 40,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        // Draw the label at the start
        GUILayout.Label("Instant Screenshot", titleStyle);

        DrawResolutionSettings();
        GUILayout.Space(10);
        DrawSavePathSettings();
        GUILayout.Space(10);
        DrawCameraSettings();
        GUILayout.Space(10);
        DrawDefaultOptions();
        GUILayout.Space(10);
        DrawScreenshotOptions();

        // End the scroll view and the padding
        GUILayout.EndScrollView();
        GUILayout.Space(10);

        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    private void DrawResolutionSettings()
    {
        EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);
        resWidth = EditorGUILayout.IntField("Width", resWidth);
        resHeight = EditorGUILayout.IntField("Height", resHeight);
        EditorGUILayout.Space();
        scale = EditorGUILayout.IntSlider("Scale", scale, 1, 15);
        EditorGUILayout.HelpBox("The default mode of screenshot is crop - so choose a proper width and height. The scale is a factor to multiply or enlarge the renders without losing quality.", MessageType.None);
        EditorGUILayout.Space();
    }

    private void DrawSavePathSettings()
    {
        GUILayout.Label("Save Path", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(savePath, GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            savePath = EditorUtility.SaveFolderPanel("Path to Save Images", savePath, Application.dataPath);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("Choose the folder in which to save the screenshots", MessageType.None);
        EditorGUILayout.Space();
    }

    private void DrawCameraSettings()
    {
        GUILayout.Label("Select Camera", EditorStyles.boldLabel);
        screenshotCamera = EditorGUILayout.ObjectField(screenshotCamera, typeof(Camera), true) as Camera;
        if (screenshotCamera == null)
        {
            screenshotCamera = Camera.main;
        }
        isTransparent = EditorGUILayout.Toggle("Transparent Background", isTransparent);
        EditorGUILayout.HelpBox("Choose the camera to capture the render. You can make the background transparent using the transparency option.", MessageType.None);
        EditorGUILayout.Space();
    }

    private void DrawDefaultOptions()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set To Screen Size", GUILayout.Height(30)))
        {
            resWidth = (int)Handles.GetMainGameViewSize().x;
            resHeight = (int)Handles.GetMainGameViewSize().y;
        }
        if (GUILayout.Button("Default Size", GUILayout.Height(30)))
        {
            resWidth = 2560;
            resHeight = 1440;
            scale = 1;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    private void DrawScreenshotOptions()
    {
        EditorGUILayout.LabelField($"Screenshot will be taken at {resWidth * scale} x {resHeight * scale} px", EditorStyles.boldLabel);
        if (GUILayout.Button("Take Screenshot", GUILayout.MinHeight(60)))
        {
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = EditorUtility.SaveFolderPanel("Path to Save Images", savePath, Application.dataPath);
                Debug.Log("Path Set");
                TakeHighResShot();
            }
            else
            {
                TakeHighResShot();
            }
        }
        EditorGUILayout.Space();
        DrawActionButtons();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Last Screenshot", GUILayout.MaxWidth(160), GUILayout.MinHeight(40)))
        {
            if (!string.IsNullOrEmpty(lastScreenshot))
            {
                Application.OpenURL("file://" + lastScreenshot);
                Debug.Log("Opening File " + lastScreenshot);
            }
        }
        if (GUILayout.Button("Open Folder", GUILayout.MaxWidth(100), GUILayout.MinHeight(40)))
        {
            Application.OpenURL("file://" + savePath);
        }
        if (GUILayout.Button("More Assets", GUILayout.MaxWidth(100), GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/5951");
        }
        if (GUILayout.Button("Github", GUILayout.MaxWidth(100), GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://github.com/saadnkhawaja/instant-screenshot-unity");
        }
        EditorGUILayout.EndHorizontal();
    }

    private string GenerateScreenshotName(int width, int height)
    {
        string fileName = string.Format("{0}/screen_{1}x{2}_{3}.png", savePath, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        lastScreenshot = fileName;
        return fileName;
    }

    public void TakeHighResShot()
    {
        Debug.Log("Taking Screenshot");
        takeHighResShot = true;
    }

    private void Update()
    {
        if (takeHighResShot)
        {
            int resWidthN = resWidth * scale;
            int resHeightN = resHeight * scale;
            RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
            screenshotCamera.targetTexture = rt;

            TextureFormat tFormat = isTransparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;
            Texture2D screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
            screenshotCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
            screenshotCamera.targetTexture = null;
            RenderTexture.active = null;
            byte[] bytes = screenShot.EncodeToPNG();
            string fileName = GenerateScreenshotName(resWidthN, resHeightN);
            System.IO.File.WriteAllBytes(fileName, bytes);
            Debug.Log($"Took screenshot to: {fileName}");
            takeHighResShot = false;
        }
    }
}
