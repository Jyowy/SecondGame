using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : MonoBehaviour {
    
    public string m_titleSceneName;
    public string m_loadingSceneName;
    public string m_resultsSceneName;
    public CanvasGroup m_fadeLayer;
    public float m_fadeTime;
    public Camera m_masterCamera;

    private string m_raceSceneLoadedName;

    private enum FadeState
    {
        None,
        FadingIn,
        Waiting,
        FadingOut
    };

    private enum FadeMode
    {
        FadeIn,
        FadeOut
    };

    private FadeState m_fadeState;
    private float k_alphaFadeInc;

    private void Start()
    {
        m_masterCamera = FindObjectOfType<Camera>();
        m_fadeState = FadeState.None;
        LoadTitleScene();
        k_alphaFadeInc = 1.0f / m_fadeTime * 2.0f;
    }

    private void LoadTitleScene()
    {
        StartCoroutine(LoadScene(m_titleSceneName));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return StartCoroutine(Fade(FadeMode.FadeIn));
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));
        yield return StartCoroutine(Fade(FadeMode.FadeOut));
    }

    private IEnumerator Fade(FadeMode fadeMode)
    {
        if (m_fadeState == FadeState.None && fadeMode == FadeMode.FadeIn)
        {
            m_fadeLayer.blocksRaycasts = true;
            m_fadeState = FadeState.FadingIn;
            while (!Mathf.Approximately(m_fadeLayer.alpha, 1.0f))
            {
                m_fadeLayer.alpha = Mathf.Min(1.0f, m_fadeLayer.alpha + k_alphaFadeInc * Time.deltaTime);
                yield return null;
            }
            m_fadeState = FadeState.Waiting;
        }
        else if (m_fadeState == FadeState.Waiting && fadeMode == FadeMode.FadeOut)
        {
            m_fadeState = FadeState.FadingOut;
            while (!Mathf.Approximately(m_fadeLayer.alpha, 0.0f))
            {
                m_fadeLayer.alpha = Mathf.Max(0.0f, m_fadeLayer.alpha - k_alphaFadeInc * Time.deltaTime);
                yield return null;
            }
            m_fadeState = FadeState.None;
            m_fadeLayer.blocksRaycasts = false;
        }
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        Scene newSceneLoaded = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newSceneLoaded);
    }

    private IEnumerator LoadRace(string raceSceneName, RaceManager.RaceMode raceMode)
    {
        m_raceSceneLoadedName = raceSceneName;

        yield return StartCoroutine(Fade(FadeMode.FadeIn));
        yield return SceneManager.UnloadSceneAsync(m_titleSceneName);
        yield return SceneManager.LoadSceneAsync(m_loadingSceneName, LoadSceneMode.Additive);
        yield return SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
        yield return StartCoroutine(Fade(FadeMode.FadeOut));

        yield return SceneManager.LoadSceneAsync(raceSceneName, LoadSceneMode.Additive);
        FindObjectOfType<RaceManager>().SetUpRaceManager(raceMode);

        yield return StartCoroutine(Fade(FadeMode.FadeIn));
        yield return SceneManager.UnloadSceneAsync(m_loadingSceneName);
        yield return SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
        FindObjectOfType<RaceManager>().ChangeState(RaceManager.RaceState.BeforeStartAnimation);
        yield return StartCoroutine(Fade(FadeMode.FadeOut));

    }

    private IEnumerator LoadResultsScreen()
    {
        yield return SceneManager.LoadSceneAsync(m_resultsSceneName, LoadSceneMode.Additive);
    }

    public IEnumerator UnloadRace()
    {
        yield return StartCoroutine(Fade(FadeMode.FadeIn));
        if (SceneManager.GetSceneByName(m_resultsSceneName).isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(m_resultsSceneName);
        }
        yield return SceneManager.UnloadSceneAsync(m_raceSceneLoadedName);
        yield return SceneManager.LoadSceneAsync(m_titleSceneName, LoadSceneMode.Additive);
        yield return StartCoroutine(Fade(FadeMode.FadeOut));
    }

    public void LoadRaceFromTitleScreen(string raceSceneName, RaceManager.RaceMode raceMode)
    {
        Debug.Log("LoadRaceFromTitleScreen " + raceSceneName + ", " + raceMode.ToString());
        StartCoroutine(LoadRace(raceSceneName, raceMode));
    }

    public void UnloadRaceToTitleScreen()
    {
        StartCoroutine(UnloadRace());
    }

    public void LoadResultsScreenInRace()
    {
        StartCoroutine(LoadResultsScreen());
    }

    public void EnableMasterCamera(bool enabled)
    {
        if (m_masterCamera != null)
        {
            m_masterCamera.enabled = enabled;
        }
    }

}
