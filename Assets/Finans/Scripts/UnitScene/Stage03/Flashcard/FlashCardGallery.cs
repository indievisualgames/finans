#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define PRE_UNITY_5_3
#endif

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if !PRE_UNITY_5_3
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Ricimi;
using static IFirestoreEnums;
using System;
using GooglePlayGames.BasicApi;
#endif

namespace UI.Pagination.Examples
{
    class FlashCardGallery : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject viewPort;
        [SerializeField] private GameObject galleryPagePrefab;
        [SerializeField] private GameObject noGallery;
        [SerializeField] private GameObject pagination;
        [SerializeField] private GameObject buttonPrefabFP;
        [SerializeField] private GameObject buttonPrefabPP;
        [SerializeField] private GameObject buttonPrefabNP;
        [SerializeField] private GameObject buttonPrefabLP;
        [SerializeField] private GameObject buttonTemplateCP;
        [SerializeField] private GameObject buttonTemplateDP;
        [SerializeField] private GameObject buttonTemplateOP;
       // [SerializeField] private GameObject triviaQuizButton;
       // [SerializeField] private GameObject triviaQuizButtonDisabled;
        [SerializeField] private GameObject triviaScreen;
        [Header("Private Fields")]
        private string buttonName = "";
        private string unitLevel = "";
        private string _context = "FlashCardGallery";
        [Header("Public Fields")]
        [SerializeField] public HashSet<int> expired = new HashSet<int>();
        [SerializeField] public bool triviaQuizStatus = false;
        public class Cards
        {
            public string Title;
            public string SubTitle;
            public string Description;
        }
        public GameObject gallery;

        //GalleryCard _galleryCard;

        void Start()
        {

            foreach (KeyValuePair<string, string> button in PlayerInfo.UnitButtonInfo)
            {
                unitLevel = button.Key;
                buttonName = button.Value;
            }
            if (triviaQuizStatus)
            {
              //  triviaQuizButton.SetActive(true);
              //  triviaQuizButtonDisabled.SetActive(false);
            }
            else
            {
               // triviaQuizButton.SetActive(false);
              //  triviaQuizButtonDisabled.SetActive(true);
            }

            Logger.LogInfo($"Found total expired card:-> {expired.Count}", _context);
            for (int i = 0; i < expired.Count; i++)
            {
                Logger.LogInfo($"Expired card number are {expired.ToArray()[i]}", _context);
            }
            if (expired.Count > 0) { CreatePages(); } else { pagination.SetActive(false); noGallery.SetActive(true); Logger.LogInfo("No viewed cards available for gallery", _context); }

        }
        void CreatePages()
        {
            PagedRect _pageRect = gallery.GetComponent<PagedRect>();
            GameObject ButtonFPGO = Instantiate(buttonPrefabFP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _pageRect.Button_FirstPage = ButtonFPGO.GetComponent<PaginationButton>();
            ButtonFPGO.GetComponent<Button>().onClick.AddListener(_pageRect.ShowFirstPage);
            ButtonFPGO.transform.SetParent(pagination.transform, false);
            GameObject ButtonPPGO = Instantiate(buttonPrefabPP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _pageRect.Button_PreviousPage = ButtonPPGO.GetComponent<PaginationButton>();
            ButtonPPGO.GetComponent<Button>().onClick.AddListener(_pageRect.PreviousPage);
            ButtonPPGO.transform.SetParent(pagination.transform, false);
            for (int i = 0; i < expired.Count; i++)
            {
                GameObject galleryPageGO = Instantiate(galleryPagePrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
                galleryPageGO.name = "Page " + (i + 1);
                GalleryCard _galleryCard = galleryPageGO.GetComponent<GalleryCard>();

                GetStreamingAssetRefAndLoad(_galleryCard, expired.ToList()[i]);
                if (i != 0) { galleryPageGO.SetActive(false); }


                galleryPageGO.transform.SetParent(viewPort.transform, false);
                _pageRect.Pages.Add(galleryPageGO.GetComponent<Page>());
            }
            GameObject ButtonCPGO = Instantiate(buttonTemplateCP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _pageRect.ButtonTemplate_CurrentPage = ButtonCPGO.GetComponent<PaginationButton>();
            ButtonCPGO.transform.SetParent(pagination.transform, false);
            GameObject ButtonOPGO = Instantiate(buttonTemplateOP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _pageRect.ButtonTemplate_OtherPages = ButtonOPGO.GetComponent<PaginationButton>();
            ButtonOPGO.transform.SetParent(pagination.transform, false);
            GameObject ButtonDPGO = Instantiate(buttonTemplateDP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _pageRect.ButtonTemplate_DisabledPage = ButtonDPGO.GetComponent<PaginationButton>();
            ButtonDPGO.transform.SetParent(pagination.transform, false);


            GameObject ButtonNPGO = Instantiate(buttonPrefabNP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _pageRect.Button_NextPage = ButtonNPGO.GetComponent<PaginationButton>();
            ButtonNPGO.GetComponent<Button>().onClick.AddListener(_pageRect.NextPage);
            ButtonNPGO.transform.SetParent(pagination.transform, false);
            GameObject ButtonLPGO = Instantiate(buttonPrefabLP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            _pageRect.Button_LastPage = ButtonLPGO.GetComponent<PaginationButton>();
            ButtonLPGO.GetComponent<Button>().onClick.AddListener(_pageRect.ShowLastPage);
            ButtonLPGO.transform.SetParent(pagination.transform, false);

            _pageRect.SetCurrentPage(1);
            _pageRect.UpdatePagination();

        }
        void GetStreamingAssetRefAndLoad(GalleryCard _galleryCard, int cardnumber)
        {
            string url_image = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/images/{cardnumber}.png";
            string url_json = $"{Application.streamingAssetsPath}/unit/{unitLevel}/{buttonName}/json/{cardnumber}.json";

            StartCoroutine(LoadJSON(url_json, _galleryCard));
            StartCoroutine(LoadImage(url_image, _galleryCard));

        }
        void CreateButtons()
        {
            GameObject ButtonFPGO = Instantiate(buttonPrefabFP, new Vector3(0f, 0f, 0f), Quaternion.identity);

            ButtonFPGO.transform.SetParent(pagination.transform, false);
            GameObject ButtonPPGO = Instantiate(buttonPrefabPP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            ButtonPPGO.transform.SetParent(pagination.transform, false);

            GameObject ButtonNPGO = Instantiate(buttonPrefabNP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            ButtonNPGO.transform.SetParent(pagination.transform, false);
            GameObject ButtonLPGO = Instantiate(buttonPrefabLP, new Vector3(0f, 0f, 0f), Quaternion.identity);
            ButtonLPGO.transform.SetParent(pagination.transform, false);


        }

        IEnumerator LoadJSON(string JSONUrl, GalleryCard _galleryCard)
        {
            Logger.LogInfo($"Loading json for gallery cards from {JSONUrl}", _context);
            UnityWebRequest request = UnityWebRequest.Get(JSONUrl);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Logger.LogInfo($"Loaded json as {request.downloadHandler.text}", _context);
                Cards card = JsonUtility.FromJson<Cards>(request.downloadHandler.text);
                _galleryCard.Title = card.Title; _galleryCard.SubTitle = card.SubTitle; _galleryCard.Description = card.Description;
            }
            else
            {
                Logger.LogError(request.error, _context);
            }
        }
        IEnumerator LoadImage(string url, GalleryCard __galleryCard)
        {
            UnityWebRequest uwr = new UnityWebRequest(url);
            DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
            uwr.downloadHandler = texDl;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D t = texDl.texture;
                Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                    Vector2.zero, 1f);
                __galleryCard.CardImg.sprite = s;
            }
        }


        public void ShowQuiz()
        {
            gallery.SetActive(false);
            triviaScreen.SetActive(true);
        }
        public void ReplayGameLevel()
        {
            string __unitLevel = unitLevel;
            PlayerInfo.GameLevel = __unitLevel;
            PlayerInfo.UnitButtonInfo.Clear();
            PlayerInfo.UnitButtonInfo.Add(__unitLevel, UnitButtonName.maingame.ToString()); if (FetchLevelDetails())
            {
                PlayerInfo.TriviaPassCollection = true;
                Logger.LogInfo($"Replaying maingame level {PlayerInfo.GameLevel} to collect trivia pass", "FlashCardGallery");
                Transition.LoadLevel(GameSceneName[(int)UnitStageButtonStatus.flashcard].ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
            }
            else
            {
                Logger.LogInfo($"Cannot replay game level {PlayerInfo.GameLevel}, no life left. Watch video to gain life", "FlashCardGallery");
                Transition.LoadLevel(SceneName.MainGame.ToString(), Params.SceneTransitionDuration, Params.SceneTransitionColor);
            }
        }

        public bool FetchLevelDetails()
        {
            Dictionary<string, object> unitStageFSData = FirestoreDatabase.GetFirestoreChildFieldData(FSMapField.unit_stage_data.ToString());
            Dictionary<string, object> currentUnitData = (Dictionary<string, object>)unitStageFSData[$"unit{unitLevel}"];
            Dictionary<string, object> mainGameData = (Dictionary<string, object>)currentUnitData[FSMapField.maingame.ToString()];
            Dictionary<string, object> gameLevelData = (Dictionary<string, object>)((Dictionary<string, object>)mainGameData[FSMapField.levels.ToString()])[unitLevel.ToString()];
            Inference.SetPlayerInfoForPCollected(gameLevelData);
            int life = PointSystem.Life = Convert.ToInt32(gameLevelData[MainGame.life.ToString()]);
            if (life == 0)
            {
                PlayerInfo.NeedPlayerLifeReset = true;
                Logger.LogInfo("Need to watch video to gain life", _context);
                return false;
            }
            return true;
        }
    }

}
