using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PGeladosSetup
{
    [MenuItem("P-Gelados/Configurar cena (Fase 1)")]
    static void Configurar()
    {
        if (TMP_Settings.instance == null)
        {
            Debug.LogError("Importe o TMP Essentials: Window > TextMeshPro > Import TMP Essential Resources, e rode de novo.");
            return;
        }

        // 1) Triggers: peixes, focas/obstaculos e iglu
        foreach (var f in Object.FindObjectsByType<Fish>(FindObjectsSortMode.None))
            SetTrigger(f.gameObject);
        foreach (var d in Object.FindObjectsByType<Damager>(FindObjectsSortMode.None))
            SetTrigger(d.gameObject);

        var iglu = GameObject.Find("Igloo");
        if (iglu != null)
        {
            if (iglu.GetComponent<Igloo>() == null) Undo.AddComponent<Igloo>(iglu);
            SetTrigger(iglu);
        }
        else Debug.LogWarning("Objeto 'Igloo' nao encontrado.");

        // 2) Jogador: acha o PlayerController que tem Rigidbody e trava a rotacao
        Transform jogador = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            var rb = pc.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning("PlayerController sem Rigidbody em '" + pc.name + "': remova este componente.", pc);
                continue;
            }
            Undo.RecordObject(rb, "Freeze rotation");
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            jogador = pc.transform;
        }
        if (jogador == null)
        {
            Debug.LogError("Nao achei um PlayerController com Rigidbody. Confira o passo 1.");
            return;
        }

        // 3) Camera
        var cam = Object.FindFirstObjectByType<CameraFollow>();
        if (cam != null)
        {
            Undo.RecordObject(cam, "Camera target");
            cam.target = jogador;
        }

        // 4) HUD + GameManager (so se ainda nao existir)
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGO, "Criar HUD");
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var score = CriarTexto(canvasGO.transform, "ScoreText", "Peixes: 0 / 10",
                new Vector2(0, 1), new Vector2(40, -40), TextAlignmentOptions.TopLeft, 48);
            var lives = CriarTexto(canvasGO.transform, "LivesText", "Vidas: 3",
                new Vector2(1, 1), new Vector2(-40, -40), TextAlignmentOptions.TopRight, 48);
            var msg = CriarTexto(canvasGO.transform, "MessageText", "",
                new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center, 64);

            var gmGO = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gmGO, "Criar GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.scoreText = score;
            gm.livesText = lives;
            gm.messageText = msg;
            gm.player = jogador;
        }
        else Debug.Log("GameManager ja existe: HUD nao foi recriado.");

        // 5) Build Settings: usa a cena aberta
        var cena = EditorSceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(cena.path))
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(cena.path, true) };

        EditorSceneManager.MarkSceneDirty(cena);
        Debug.Log("P-Gelados: cena configurada. Salve com Cmd+S e dê Play.");
    }

    static void SetTrigger(GameObject go)
    {
        foreach (var c in go.GetComponents<Collider>())
        {
            Undo.RecordObject(c, "Trigger");
            c.isTrigger = true;
        }
    }

    static TMP_Text CriarTexto(Transform pai, string nome, string texto, Vector2 ancora,
                               Vector2 pos, TextAlignmentOptions alin, float tamanho)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = ancora;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900, 160);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = texto;
        t.fontSize = tamanho;
        t.alignment = alin;
        t.color = new Color(0.05f, 0.15f, 0.35f);
        return t;
    }
}