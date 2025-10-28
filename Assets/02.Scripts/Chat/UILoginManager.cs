using BackEnd;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UILoginManager : MonoBehaviour
{
    struct UserTokenJson
    {
        public string id;
        public string pw;
        public string language;
    }

    public Toggle MultiCharacter = null;

    public Toggle CustomAuth = null;

    public InputField Username = null;

    public InputField Password = null;

    public InputField Property = null;

    public Button LoginButton = null;

    // Start is called before the first frame update
    void Start()
    {
        if (LoginButton != null)
        {
            LoginButton.onClick.AddListener(Login);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Login()
    {
        UserTokenJson userTokenJson = new UserTokenJson();

        if (Username != null && Password != null)
        {
            if (Username.text.Length > 0 && Password.text.Length > 0)
            {
                userTokenJson.id = Username.text;
                userTokenJson.pw = Password.text;

                bool isCustomAuth = false;

                if (CustomAuth != null)
                {
                    isCustomAuth = CustomAuth.isOn;
                }

                if (!isCustomAuth)
                {
                    Backend.Initialize();

                #if UNITY_ANDROID
                    Debug.Log("구글 해시 : " + Backend.Utils.GetGoogleHash());
                #endif
                }

                if (Property != null)
                {
                    if (Property.text.Length > 0)
                    {
                        List<string> list = new List<string>();
                        list.AddRange(Property.text.Split(','));

                        if (list.Count == 4)
                        {
                            string language = list[3].Replace("\n","").Replace(" ", "");

                            if (isCustomAuth)
                            {
                                userTokenJson.language = language;
                            } 
                            else
                            {
                                Backend.LocationProperties.CustomizeLocationProperties(list[0], list[1], list[2], language);
                            }
                        }

                        //Backend.LocationProperties.CustomizeLocationProperties("Seoul", "South Korea", "Seoul", "ko-KR");

                        //Seoul, South Korea, Seoul, ko-KR
                        //New York, United States, New York, en-US
                        //Tokyo, Japan, Tokyo, ja-JP
                        //Beijing, China, Beijing, zh-CN
                    }
                }

                if (!isCustomAuth)
                { 
                    bool isMultiCharacter = false;

                    if (MultiCharacter != null)
                    {
                        isMultiCharacter = MultiCharacter.isOn;
                    }

                    if (isMultiCharacter)
                    {
                        var returnObject = Backend.MultiCharacter.Account.LoginAccount(Username.text, Password.text);

                        if (returnObject.IsSuccess())
                        {
                            Debug.Log("계정 로그인 : " + returnObject);
                        }
                        else
                        {
                            Debug.LogError("계정 로그인 : " + returnObject);

                            returnObject = Backend.MultiCharacter.Account.CreateAccount(Username.text, Password.text);

                            if (returnObject.IsSuccess())
                            {
                                Debug.Log("계정 생성 : " + returnObject);
                            }
                            else
                            {
                                Debug.LogError("계정 생성 : " + returnObject);
                                return;
                            }

                            returnObject = Backend.MultiCharacter.Character.CreateCharacter(Username.text);

                            if (returnObject.IsSuccess())
                            {
                                Debug.Log("캐릭터 생성 : " + returnObject);
                            }
                            else
                            {
                                Debug.LogError("캐릭터 생성 : " + returnObject);
                                return;
                            }
                        }

                        var bro = Backend.MultiCharacter.Character.GetCharacterList();

                        if (bro.IsSuccess())
                        {
                            Debug.Log("캐릭터 리스트 불러오기 : " + bro);
                        }
                        else
                        {
                            Debug.LogError("캐릭터 리스트 불러오기 : " + bro);
                            return;
                        }

                        LitJson.JsonData characterJson = bro.GetReturnValuetoJSON()["characters"][0];

                        string uuid = characterJson["uuid"].ToString();
                        string inDate = characterJson["inDate"].ToString();

                        Debug.Log($"캐릭터 정보 : uuid : {uuid} / inDate : {inDate}");

                        var bro2 = Backend.MultiCharacter.Character.SelectCharacter(uuid, inDate);

                        if (bro2.IsSuccess())
                        {
                            Debug.Log("캐릭터 로그인 성공 : " + bro2);
                        }
                        else
                        {
                            Debug.LogError("캐릭터 로그인 실패 : " + bro2);
                            return;
                        }
                    }
                    else
                    {
                        var returnObject = Backend.BMember.CustomLogin(Username.text, Password.text);
                        if (returnObject.IsSuccess())
                        {
                            Debug.Log("로그인 성공 : " + returnObject);
                        }
                        else
                        {
                            returnObject = Backend.BMember.CustomSignUp(Username.text, Password.text);
                            if (returnObject.IsSuccess())
                            {
                                Debug.Log("회원가입 성공 : " + returnObject);
                            }
                            else
                            {
                                Debug.LogError("회원가입 실패 : " + returnObject);
                                return;
                            }

                            var bro = Backend.BMember.UpdateNickname(Username.text);
                            if (bro.IsSuccess())
                            {
                                Debug.Log("닉네임 변경 성공 : " + bro);
                            }
                            else
                            {
                                Debug.LogError("닉네임 변경 실패 : " + bro);
                                return;
                            }

                            Debug.Log("로그인 성공 : " + returnObject);
                        }
                    }

                    GameManager.Instance.MyNickname = BackEnd.Backend.GetBackendChatSettings().nickname;
                }
                else
                {
                    GameManager.Instance.UserToken = JsonUtility.ToJson(userTokenJson);
                    GameManager.Instance.MyNickname = Username.text;
                }

                Debug.Log("채팅을 시작합니다");
                GameManager.Instance.StartBackendChat();
            }
        }
    }
}
