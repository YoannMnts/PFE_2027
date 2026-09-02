using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HelteixInstallerWindow : EditorWindow
{
    private const string RequiredBlockHeader = "[npmAuth.\"https://upm.pckgs.io\"]";
    private const string RequiredToken = "ROpzWWo77U22Sy+Sh/W5996JHWGpJ3OLIqBp3jq9eKo=";
    private const string RequiredAlwaysAuth = "true";

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private Button joinDiscordButton;
    private Button setupButton;

    private Button copyNameButton;
    private Button copyLinkButton;
    private Button copyScopesButton;


    [MenuItem("Tools/Helteix/Setup")]
    public static void Open()
    {
        HelteixInstallerWindow wnd = GetWindow<HelteixInstallerWindow>();
        wnd.titleContent = new GUIContent("Helteix");
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OpenOnProjectLoad()
    {
        if (!IsSetup() && !SessionState.GetBool("HelteixInstallerAutoOpen", false))
        {
            SessionState.SetBool("HelteixInstallerAutoOpen", true);
            Open();
        }
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement window = m_VisualTreeAsset.Instantiate();
        root.Add(window);

        joinDiscordButton = window.Q<Button>("DiscordButton");
        setupButton = window.Q<Button>("SetupButton");

        copyNameButton = window.Q<Button>("CopyName");
        copyLinkButton = window.Q<Button>("CopyURL");
        copyScopesButton = window.Q<Button>("CopyScope");


        joinDiscordButton.clicked += JoinDiscord;
        setupButton.clicked += Setup;

        copyNameButton.clicked += () =>
        {
            EditorGUIUtility.systemCopyBuffer = "Helteix";
            Debug.Log("Copied to clipboard!");
        };

        copyLinkButton.clicked += () =>
        {
            EditorGUIUtility.systemCopyBuffer = "https://upm.pckgs.io";
            Debug.Log("Copied to clipboard!");
        };

        copyScopesButton.clicked += () =>
        {
            EditorGUIUtility.systemCopyBuffer = "com.helteix";
            Debug.Log("Copied to clipboard!");
        };

        setupButton.SetEnabled(!IsSetup());
    }


    private static bool IsSetup()
    {
        if(SessionState.GetBool("Helteix_Setup", false))
            return true;
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string configPath = Path.Combine(userProfile, ".upmconfig.toml");

        if (!File.Exists(configPath))
            return false;

        string text = File.ReadAllText(configPath);

        bool isSetup = text.Contains(RequiredBlockHeader) && text.Contains(RequiredToken);

        if(isSetup)
            return false;

        return true;
    }

    private void Setup()
    {
        if(IsSetup())
            return;
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string configPath = Path.Combine(userProfile, ".upmconfig.toml");

        if (!File.Exists(configPath))
        {
            File.WriteAllText(configPath, string.Empty);
            Debug.Log($".upmconfig.toml créé à : {configPath}");
        }

       var lines = new List<string>(File.ReadAllLines(configPath));

        int headerIndex = lines.FindIndex(l => l.Trim() == RequiredBlockHeader);

        // Si le header n'existe pas, on l'ajoute (avec les clés) à la fin
        if (headerIndex == -1)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
                lines.Add(string.Empty); // séparation propre

            lines.Add(RequiredBlockHeader);
            lines.Add($"token = \"{RequiredToken}\"");
            lines.Add("alwaysAuth = true");

            File.WriteAllLines(configPath, lines);
            Debug.Log("Bloc ajouté à .upmconfig.toml");
            return;
        }

        // Le header existe -> on parcourt le bloc pour remplacer ou repérer les clés existantes
        bool tokenFound = false;
        bool alwaysAuthFound = false;

        // j sera l'indice du premier header suivant (ou lines.Count si EOF)
        int j = headerIndex + 1;
        for (; j < lines.Count; j++)
        {
            string current = lines[j];

            // si on tombe sur un autre header, on sort (fin du bloc)
            if (current.TrimStart().StartsWith("["))
                break;

            string trimmed = current.TrimStart();
            if (trimmed.StartsWith("token ="))
            {
                // on remplace la ligne par la bonne valeur (évite la duplication)
                lines[j] = $"token = \"{RequiredToken}\"";
                tokenFound = true;
                continue;
            }

            if (trimmed.StartsWith("alwaysAuth ="))
            {
                lines[j] = "alwaysAuth = true";
                alwaysAuthFound = true;
                continue;
            }
        }

        // On insère les clés manquantes juste avant j (donc avant le prochain header ou EOF)
        var toInsert = new List<string>();
        if (!tokenFound) toInsert.Add($"token = \"{RequiredToken}\"");
        if (!alwaysAuthFound) toInsert.Add("alwaysAuth = true");

        if (toInsert.Count > 0)
            lines.InsertRange(j, toInsert);

        File.WriteAllLines(configPath, lines);
        Debug.Log(".upmconfig.toml vérifié et mis à jour si nécessaire");

    }

    private void JoinDiscord()
    {
        Application.OpenURL("https://discord.gg/Cx4wEUw7");
    }
}