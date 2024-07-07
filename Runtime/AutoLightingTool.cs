#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AutoLightingTool : EditorWindow
{
    [Header("Settings")]
    [Tooltip("The root transform of the building.")]
    public Transform buildingRoot;
    
    [Tooltip("Adjust the intensity of the lights.")]
    public float lightIntensity = 0.16f;
    
    [Tooltip("Choose the color of the lights.")]
    public Color lightColor = Color.white;
    
    [Tooltip("Set the maximum number of point lights.")]
    public int maxLights = 50;
    
    [Tooltip("Define the radius of the point lights.")]
    public float pointLightRadius = 10f;
    
    [Tooltip("Specify the vertical offset of the point lights.")]
    public float pointLightVerticalOffset = 5f;
    
    [Tooltip("Toggle to enable/disable shadows for the lights.")]
    public bool enableShadows = false; // Added setting for shadows

    private int totalItems;
    private int processedItems;
    private GameObject lightsContainer;

    [MenuItem("Shadow Byte SDK/Auto Lighting Tool")]
    public static void ShowWindow()
    {
        GetWindow<AutoLightingTool>("Auto Lighting Tool");
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Auto Lighting Tool", EditorStyles.boldLabel);
        GUILayout.Space(30); // Add some vertical space
        GUILayout.EndHorizontal();

        GUILayout.Label("Lighting Build Settings", EditorStyles.boldLabel);
        buildingRoot = (Transform)EditorGUILayout.ObjectField(
            new GUIContent("Building Root", "Specify the root transform of the building."),
            buildingRoot,
            typeof(Transform),
            true
        );

        lightIntensity = EditorGUILayout.FloatField(
            new GUIContent("Light Intensity", "Adjust the intensity of the lights."),
            lightIntensity
        );

        lightColor = EditorGUILayout.ColorField(
            new GUIContent("Light Color", "Choose the color of the lights."),
            lightColor
        );

        maxLights = EditorGUILayout.IntField(
            new GUIContent("Max Point Lights", "Set the maximum number of point lights."),
            maxLights
        );

        pointLightRadius = EditorGUILayout.FloatField(
            new GUIContent("Point Light Radius", "Define the radius of the point lights."),
            pointLightRadius
        );

        pointLightVerticalOffset = EditorGUILayout.FloatField(
            new GUIContent("Point Light Vertical Offset", "Specify the vertical offset of the point lights."),
            pointLightVerticalOffset
        );

        enableShadows = EditorGUILayout.Toggle(
            new GUIContent("Enable Shadows", "Toggle to enable/disable shadows for the lights."),
            enableShadows
        );

        if (GUILayout.Button("Start"))
        {
            StartAutoLighting();
        }

        // Centered text header
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Useful links", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // Buttons for links
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Website", "Visit My Website")))
        {
            Application.OpenURL("https://shadowbyte.dev/");
        }

        if (GUILayout.Button(new GUIContent("Gumroad", "Visit My Gumroad")))
        {
            Application.OpenURL("https://shadowbytedev.gumroad.com/");
        }

        if (GUILayout.Button(new GUIContent("Discord", "Join Our Discord Server")))
        {
            Application.OpenURL("https://shadowhub.dev/");
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10); // Add some space before the note

        // Note about VRChat SDK
        EditorGUILayout.HelpBox(
            "If you get a compiler error when trying to build and test or build and publish using the VRChat SDK, " +
            "just delete this script and try to build and publish or build and test again. The VRChat SDK for whatever " +
            "reason doesn’t like certain scripts and will refuse to build if it’s presented.",
            MessageType.Warning
        );

        //further support
        GUILayout.Space(10); // Add some space before the secondary message
        EditorGUILayout.HelpBox(
            "If you need further support or have any issues, please join our Discord server and open a ticket. " +
            "We’d be more than happy to assist you.",
            MessageType.Info
        );
        
        //Version Info
        GUILayout.Space(10); // Add some space before the secondary message
        EditorGUILayout.HelpBox(
            "You are using version: '1.0.0-Beta-1' of the Auto Lighting Tool For Unity. Please report any bugs to our discord server so we can quickly get them resolved",
            MessageType.Info
        );

        //Copyright Notice
        GUILayout.Space(10); // Add some space before the secondary message
        EditorGUILayout.HelpBox(
            "This tool is protected by copyright and is the intellectual property of Shadow Byte Development. Sharing, redistributing, or reselling is strictly prohibited. Any violation will result in the loss of your license. Thank you for your compliance.",
            MessageType.Error
        );

        // Copyright box
        GUILayout.Space(10); // Add some space before the copyright box
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("© 2024 Shadow Byte Development.");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
          
    }

    void StartAutoLighting()
    {
        if (buildingRoot == null)
        {
            Debug.LogError("Building root not assigned. Please assign it in the Inspector.");
            return;
        }

        // Create a container for lights
        lightsContainer = new GameObject("LightsContainer");
        lightsContainer.transform.SetParent(buildingRoot);

        // Count the number of items
        totalItems = CountItemsRecursive(buildingRoot);
        processedItems = 0;

        // Show progress bar
        EditorUtility.DisplayProgressBar("Auto Lighting", "Adding lights...", 0f);

        // Add directional light
        AddDirectionalLight();

        // Add a limited number of point lights based on child objects
        AddLimitedPointLightsRecursive(buildingRoot, maxLights);

        // Add light probes
        AddLightProbes();

        // Clear progress bar
        EditorUtility.ClearProgressBar();

        // Display confirmation dialog
        EditorUtility.DisplayDialog("Auto Lighting", "Auto Lighting process completed for " + totalItems + " objects.", "OK");
    }

    int CountItemsRecursive(Transform parent)
    {
        int count = 1; // Counting the current object
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            count += CountItemsRecursive(child);
        }
        return count;
    }

    void AddDirectionalLight()
    {
        GameObject directionalLightGO = new GameObject("Directional Light");
        directionalLightGO.transform.SetParent(lightsContainer.transform);
        Light directionalLight = directionalLightGO.AddComponent<Light>();
        directionalLight.type = LightType.Directional;
        directionalLight.intensity = lightIntensity;
        directionalLight.color = lightColor;
        directionalLight.shadows = enableShadows ? LightShadows.Soft : LightShadows.None; // Set shadows based on the enableShadows setting

        processedItems++;
        UpdateProgressBar();
    }

    void AddLimitedPointLightsRecursive(Transform parent, int maxLights)
    {
        int lightsToAdd = Mathf.Min(parent.childCount, maxLights);

        for (int i = 0; i < lightsToAdd; i++)
        {
            Transform child = parent.GetChild(i);

            // Add point light to each child
            GameObject pointLightGO = new GameObject("Point Light " + child.name);
            pointLightGO.transform.SetParent(lightsContainer.transform);
            Light pointLight = pointLightGO.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.intensity = lightIntensity;
            pointLight.color = lightColor;

            // Adjust settings for point lights
            pointLight.range = pointLightRadius;
            pointLight.transform.position = child.position + new Vector3(0f, pointLightVerticalOffset, 0f);

            processedItems++;
            UpdateProgressBar();
        }
    }

    void AddLightProbes()
    {
        LightProbeGroup lightProbeGroup = buildingRoot.gameObject.GetComponent<LightProbeGroup>();
        if (lightProbeGroup == null)
        {
            lightProbeGroup = buildingRoot.gameObject.AddComponent<LightProbeGroup>();
        }
    }

    void UpdateProgressBar()
    {
        float progress = (float)processedItems / totalItems;
        EditorUtility.DisplayProgressBar("Auto Lighting", "Adding lights...", progress);

        if (processedItems == totalItems)
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
#endif