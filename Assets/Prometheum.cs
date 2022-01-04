using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System;
using TMPro;
using UnityEngine.UI;
using epoching.easy_qr_code;

public class Prometheum : MonoBehaviour
{
    // Start is called before the first frame update
    public static string url = "http://127.0.0.1:5000/"; //This is our HOST
    public static char[] chars = { 'a', 'b', 'c', 'v', 'e', 'w', 'r', 't', 'y', 'u', 'ı', 'o', 'p', 'g', 'm', 'ö', 'l', 'i', 'q', 'w', 'z', '3', '4', '5', '6', '7', '8', '9' };
    System.Random random = new System.Random();

    public Account account;
    public TMP_InputField public_address;
    public TMP_InputField private_key;
    public Button messageButton;
    public Button mineButton;
    public Button walletButton;
    public TextMeshProUGUI proCoinText;
    public Image mineMessage;
    public Image saveWarningMessage;
    public RawImage qr_image;
    public RawImage qr_image2;
    public Image transactionIssueMessage;

    public GameObject mainScreen;
    public GameObject messageScreen;
    public GameObject listMessagesScreen;
    public GameObject walletScreen;
    public GameObject transactionScreen;
    public GameObject scanQRObject;
    public GameObject createPaymentScreen;


    public TMP_InputField otherPublicAddress;
    public TMP_InputField messageBox;
    public VerticalLayoutGroup listView;
    public VerticalLayoutGroup listView2;
    public GameObject scrollbar;
    public GameObject exButton;
    public GameObject addAddressBackground;
    public TMP_InputField createPaymentInputField;



    void Start()
    {
        account = new Account();
        messageButton.gameObject.SetActive(false);
        mineButton.gameObject.SetActive(false);
        walletButton.gameObject.SetActive(false);
        //PlayerPrefs.DeleteAll();

        if (PlayerPrefs.HasKey("address") && PlayerPrefs.HasKey("private_key"))
        {
            public_address.text = PlayerPrefs.GetString("address");
            private_key.text = PlayerPrefs.GetString("private_key");
            messageButton.gameObject.SetActive(true);
            mineButton.gameObject.SetActive(true);
            walletButton.gameObject.SetActive(true);
            try
            {
                GetAllProCoinFromAddress(PlayerPrefs.GetString("address"),proCoinText);
            }
            catch (Exception ex)
            {
                mineMessage.gameObject.SetActive(true);
                mineMessage.color = Color.red;
                mineMessage.GetComponentInChildren<TextMeshProUGUI>().text = ex.Message + " Try Agein Later:(";
                Debug.Log(ex.Message); // If see this error you have server issue!!!
            }

        }




        /*
        List<string> vs = new List<string>();
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");
        vs.Add("asashjashajshajshajsajs");

        RectTransform parents = scrollbar.GetComponent<RectTransform>();
        var label = GameObject.Find("Label");

        for (int i = 0; i < vs.Count; i++)
        {
            //GameObject t = new GameObject(vs[i]);
            //t = exButton;
            GameObject t = Instantiate(exButton, new Vector3(), Quaternion.identity);
            t.GetComponent<RectTransform>().SetParent(parents);
            t.GetComponentInChildren<Text>().fontSize = 40;
            t.GetComponentInChildren<Text>().font = label.GetComponent<Text>().font;
            t.GetComponentInChildren<Text>().color = Color.black;
            t.GetComponentInChildren<Text>().text = vs[i];
            t.gameObject.SetActive(true);

        }

        */


    }

    public string CalculatePublicAddress()
    {
        string result = "";
        var x = random.Next(500, 10000);
        for (int i = 0; i < x; i++)
        {
            var data = random.Next(0, chars.Length);
            result += chars[data];
        }

        SHA256 sHA256 = SHA256.Create();
        byte[] inBytes = Encoding.ASCII.GetBytes(result); // We take block datas in byte array 
        byte[] outBytes = sHA256.ComputeHash(inBytes); // Calculate has with SHA256 on our in-bytes
        return Convert.ToBase64String(outBytes); // Change byte array on string
    }
    public string CalculatePrivateKey()
    {
        var result = RandomKeys();

        SHA256 sHA256 = SHA256.Create();
        byte[] inBytes = Encoding.ASCII.GetBytes(result); // We take block datas in byte array 
        byte[] outBytes = sHA256.ComputeHash(inBytes); // Calculate has with SHA256 on our in-bytes
        var res1 = Convert.ToBase64String(outBytes); // Change byte array on string

        var result2 = RandomKeys();

        byte[] inBytes2 = Encoding.ASCII.GetBytes(result2); // We take block datas in byte array 
        byte[] outBytes2 = sHA256.ComputeHash(inBytes2); // Calculate has with SHA256 on our in-bytes
        var res2 = Convert.ToBase64String(outBytes2); // Change byte array on string

        return res1 + res2;
    }

    private string RandomKeys()
    {
        string result = "";
        var x = random.Next(500, 10000);
        for (int i = 0; i < x; i++)
        {
            var data = random.Next(0, chars.Length);
            result += chars[data];
        }

        return result;

    }

    public void GetChain()
    {
        var httpRequest = (HttpWebRequest)WebRequest.Create(url + "get_chain");
        httpRequest.Accept = "application/json";
        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
            Debug.Log(result);
        }
    }

    public void GenerateAccount()
    {
        try
        {
            account.address = CalculatePublicAddress();
            account.private_key = CalculatePrivateKey();

            public_address.text = account.address;
            private_key.text = account.private_key;


            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "generate_account");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(account);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Debug.Log(result);
            }
        }
        catch (Exception ex)
        {
            public_address.text = "";
            private_key.text = "";
            StartCoroutine(GenerateAccountIssue(ex));
        }

    }

    private IEnumerator GenerateAccountIssue(Exception ex)
    {
        mineMessage.gameObject.SetActive(true);
        mineMessage.color = Color.red;
        mineMessage.GetComponentInChildren<TextMeshProUGUI>().text = ex.Message + " Try Agein Later:(";
        yield return new WaitForSeconds(1.5f);
        mineMessage.gameObject.SetActive(false);


    }

    public bool ControlAccount(string address, string private_key)
    {
        var isValid = false;

        Account.Account2 account2 = new Account.Account2();
        account2.your_address = address;
        account2.your_private_key = private_key;

        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "control_account");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonConvert.SerializeObject(account2);
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
            var data = JsonConvert.DeserializeObject<Account.isValidAccount>(result);
            isValid = data.message;

        }


        return isValid;

    }

    public bool ControlAddress(string otherAddress)
    {
        var isValid = false;

        Account.Address address = new Account.Address();
        address.address = otherAddress;

        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "control_address");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonConvert.SerializeObject(address);
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
            var data = JsonConvert.DeserializeObject<Account.isValidAccount>(result);
            isValid = data.message;

        }


        return isValid;

    }

    public void SaveKeys()
    {
        if (!string.IsNullOrEmpty(public_address.text) && !string.IsNullOrEmpty(private_key.text))
        {

            var isValid = ControlAccount(public_address.text, private_key.text);
            if (isValid)
            {
                if (PlayerPrefs.HasKey("address") && PlayerPrefs.HasKey("private_key"))
                {
                    saveWarningMessage.gameObject.SetActive(true);
                }
                else
                {
                    PlayerPrefs.SetString("address", public_address.text);
                    PlayerPrefs.SetString("private_key", private_key.text);
                    messageButton.gameObject.SetActive(true);
                    mineButton.gameObject.SetActive(true);
                    walletButton.gameObject.SetActive(true);
                    StartCoroutine(ShowSaveMessage());
                    GetAllProCoinFromAddress(PlayerPrefs.GetString("address"),proCoinText);
                }
            }
            else
            {
                StartCoroutine(ShowDenisedSaveMessage());
            }


        }

    }

    public void SaveYesButton()
    {
        var isValid = ControlAccount(public_address.text, private_key.text);
        if (isValid)
        {
            saveWarningMessage.gameObject.SetActive(false);
            PlayerPrefs.SetString("address", public_address.text);
            PlayerPrefs.SetString("private_key", private_key.text);
            messageButton.gameObject.SetActive(true);
            mineButton.gameObject.SetActive(true);
            walletButton.gameObject.SetActive(true);
            StartCoroutine(ShowSaveMessage());
            GetAllProCoinFromAddress(PlayerPrefs.GetString("address"),proCoinText);
            PlayerPrefs.DeleteKey("AddressList");
        }
        else
        {
            StartCoroutine(ShowDenisedSaveMessage());
        }
    }
    public void SaveNoButton()
    {
        saveWarningMessage.gameObject.SetActive(false);
    }



    private IEnumerator ShowSaveMessage()
    {
        mineMessage.gameObject.SetActive(true);
        mineMessage.color = Color.green;
        mineMessage.GetComponentInChildren<TextMeshProUGUI>().text = "Save is Success!";
        yield return new WaitForSeconds(1.5f);
        mineMessage.gameObject.SetActive(false);
    }


    private IEnumerator ShowDenisedSaveMessage()
    {
        mineMessage.gameObject.SetActive(true);
        mineMessage.color = Color.red;
        mineMessage.GetComponentInChildren<TextMeshProUGUI>().text = "Public Address or Private Key is not valid!";
        yield return new WaitForSeconds(1.5f);
        mineMessage.gameObject.SetActive(false);
    }



    public void MessageButton()
    {
        mainScreen.gameObject.SetActive(false);
        listMessagesScreen.gameObject.SetActive(true);
        GetAllAddressFromComingMessage();
        GetAllAddressFromYourMessage();

        //messageScreen.gameObject.SetActive(true);

    }

    public void SendTransactionMessage()
    {
        if (!string.IsNullOrEmpty(otherPublicAddress.text) && !string.IsNullOrEmpty(messageBox.text))
        {
            var controlAddress = ControlAddress(otherPublicAddress.text);
            if (controlAddress && otherPublicAddress.text != PlayerPrefs.GetString("address"))
            {
                Account.Transaction transaction = new Account.Transaction();
                transaction.sender = PlayerPrefs.GetString("address");
                transaction.receiver = otherPublicAddress.text;
                transaction.amount = messageBox.text;


                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "add_transaction");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(transaction);
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Debug.Log(result);
                    messageBox.text = "";

                }



            }
        }
    }



    public void SendTransactionPro(string address, string otherAddress, string amount,TextMeshProUGUI proText)
    {
        Account.Transaction transaction = new Account.Transaction();
        transaction.sender = address;
        transaction.receiver = otherAddress;
        transaction.amount = amount;

        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "add_transaction");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonConvert.SerializeObject(transaction);
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
            GetAllProCoinFromAddress(PlayerPrefs.GetString("address"),proText);
        }


    }

    public void GetMessages()
    {


        if (!string.IsNullOrEmpty(otherPublicAddress.text) && otherPublicAddress.text != PlayerPrefs.GetString("address"))
        {
            Account.GetMessage message = new Account.GetMessage();
            message.address = PlayerPrefs.GetString("address");
            message.other_address = otherPublicAddress.text;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "get_messages");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(message);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Account.Messages>(result);

                //DELETE
                if (listView.transform.childCount > 0)
                {
                    foreach (Transform child in listView.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }

                if (listView2.transform.childCount > 0)
                {
                    foreach (Transform child in listView2.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }




                //ADD


                RectTransform parent = listView.GetComponent<RectTransform>();

                var text = listView.GetComponentInChildren<Text>();
                var label = GameObject.Find("Label");

                var count = data.receiver_message.Length;
                var isBiggerFive = false;
                for (int i = 0; i < data.receiver_message.Length; i++)
                {
                    if (count > 5 && !isBiggerFive)
                    {
                        var last = count - 5;
                        i = last;
                        isBiggerFive = true;
                    }

                    string[] controlAmount = data.receiver_message[i].Split(',');
                    if (controlAmount.Length > 0)
                    {
                        if (controlAmount[0] != "f0b6081f8b6f472b8d27ca04537e50c4d2a17d6f05fe466db0fbf89cfe1e51f0")
                        {
                            GameObject g = new GameObject(data.receiver_message[i]);
                            TextMeshProUGUI t = g.AddComponent<TextMeshProUGUI>();
                            t.GetComponent<RectTransform>().SetParent(parent);
                            t.fontSize = 40;
                            t.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 71.406f);
                            t.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1049.9f);
                            t.font = label.GetComponent<TextMeshProUGUI>().font;
                            t.color = Color.black;
                            t.text = data.receiver_message[i];
                        }
                    }
                    else
                    {
                        GameObject g = new GameObject(data.receiver_message[i]);
                        TextMeshProUGUI t = g.AddComponent<TextMeshProUGUI>();
                        t.GetComponent<RectTransform>().SetParent(parent);
                        t.fontSize = 40;
                        t.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 71.406f);
                        t.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1049.9f);
                        t.font = label.GetComponent<TextMeshProUGUI>().font;
                        t.color = Color.black;
                        t.text = data.receiver_message[i];
                    }





                }

                var count2 = data.sender_message.Length;
                RectTransform parent2 = listView2.GetComponent<RectTransform>();
                var isBiggerFive2 = false;


                for (int i = 0; i < data.sender_message.Length; i++)
                {

                    if (count2 > 5 && !isBiggerFive2)
                    {
                        var last = count2 - 5;
                        i = last;
                        isBiggerFive2 = true;
                    }

                    string[] controlAmount2 = data.sender_message[i].Split(',');

                    if (controlAmount2.Length > 0)
                    {
                        if (controlAmount2[0] != "f0b6081f8b6f472b8d27ca04537e50c4d2a17d6f05fe466db0fbf89cfe1e51f0")
                        {
                            GameObject g2 = new GameObject(data.sender_message[i]);
                            TextMeshProUGUI t2 = g2.AddComponent<TextMeshProUGUI>();
                            t2.GetComponent<RectTransform>().SetParent(parent2);
                            t2.fontSize = 40;
                            t2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 71.406f);
                            t2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1049.9f);
                            t2.font = label.GetComponent<TextMeshProUGUI>().font;
                            t2.color = Color.black;
                            t2.text = data.sender_message[i];
                        }
                    }
                    else
                    {
                        GameObject g2 = new GameObject(data.sender_message[i]);
                        TextMeshProUGUI t2 = g2.AddComponent<TextMeshProUGUI>();
                        t2.GetComponent<RectTransform>().SetParent(parent2);
                        t2.fontSize = 40;
                        t2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 71.406f);
                        t2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1049.9f);
                        t2.font = label.GetComponent<TextMeshProUGUI>().font;
                        t2.color = Color.black;
                        t2.text = data.sender_message[i];
                    }




                }


            }




        }
        else
        {

        }
    }



    private IEnumerator GetMessageEveryTwoSeconds()
    {
        GetMessages();
        yield return new WaitForSeconds(2);
        StartCoroutine(GetMessageEveryTwoSeconds());
    }
    private bool justOne = false;
    public void GetMessageButton()
    {
        if (!justOne)
            StartCoroutine(GetMessageEveryTwoSeconds());
        justOne = true;

    }

    public void TurnBackButton()
    {
        StopAllCoroutines();
        justOne = false;
        mainScreen.gameObject.SetActive(true);
        messageScreen.gameObject.SetActive(false);
        listMessagesScreen.gameObject.SetActive(false);
        walletScreen.gameObject.SetActive(false);
        createPaymentScreen.gameObject.SetActive(false);
    }


    public void AddAddressButton()
    {
        addAddressBackground.gameObject.SetActive(true);
    }


    public void AddAddress()
    {
        if (!string.IsNullOrEmpty(addAddressBackground.GetComponentInChildren<TMP_InputField>().text))
        {
            var address = addAddressBackground.GetComponentInChildren<TMP_InputField>().text;
            var controlAddress = ControlAddress(address);
            if (controlAddress)
            {
                if (PlayerPrefs.HasKey("AddressList"))
                {
                    string[] takeAddress = PlayerPrefs.GetString("AddressList").Split(',');
                    string result = "";
                    var isSame = false;
                    for (int i = 0; i < takeAddress.Length; i++)
                    {
                        result += takeAddress[i] + ",";
                        if (takeAddress[i] == address)
                            isSame = true;
                    }

                    if (!isSame)
                    {
                        result += address + ",";
                        PlayerPrefs.SetString("AddressList", result);
                        StartCoroutine(AddressValidMessage());

                    }
                    else
                    {
                        StartCoroutine(AddressInValidMessage());
                    }


                }
                else
                {
                    StartCoroutine(AddressValidMessage());
                    PlayerPrefs.SetString("AddressList", address + ",");
                }


                //StartCoroutine(AddressValidMessage());

            }
            else
            {
                StartCoroutine(AddressInValidMessage());
            }

            GetAllAddressFromYourMessage();

        }

    }

    public void ExitAddress()
    {
        addAddressBackground.gameObject.SetActive(false);
    }

    private IEnumerator AddressInValidMessage()
    {
        addAddressBackground.GetComponentInChildren<TMP_InputField>().image.color = Color.red;
        addAddressBackground.GetComponentInChildren<TMP_InputField>().text = "Address Invalid!";
        yield return new WaitForSeconds(1.5f);
        addAddressBackground.GetComponentInChildren<TMP_InputField>().image.color = Color.white;
        addAddressBackground.GetComponentInChildren<TMP_InputField>().text = "";

    }


    private IEnumerator AddressValidMessage()
    {
        addAddressBackground.GetComponentInChildren<TMP_InputField>().image.color = Color.green;
        addAddressBackground.GetComponentInChildren<TMP_InputField>().text = "Success!";
        yield return new WaitForSeconds(1.5f);
        addAddressBackground.GetComponentInChildren<TMP_InputField>().image.color = Color.white;
        addAddressBackground.GetComponentInChildren<TMP_InputField>().text = "";

    }



    public void MoveToMessageFromAddress()
    {
        listMessagesScreen.gameObject.SetActive(false);
        messageScreen.gameObject.SetActive(true);
        var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        //Debug.Log(button.GetComponent<Button>().GetComponentInChildren<Text>().text);
        otherPublicAddress.text = button.GetComponent<Button>().GetComponentInChildren<Text>().text;
        otherPublicAddress.enabled = false;
        StartCoroutine(GetMessageEveryTwoSeconds());
        justOne = true;

    }


    public void GetAllAddressFromYourMessage()
    {

        if (scrollbar.transform.childCount > 0)
        {
            foreach (Transform child in scrollbar.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        RectTransform parents = scrollbar.GetComponent<RectTransform>();
        var label = GameObject.Find("Label");

        if (PlayerPrefs.HasKey("AddressList"))
        {
            string[] address = PlayerPrefs.GetString("AddressList").Split(',');
            Debug.Log(PlayerPrefs.GetString("AddressList"));
            for (int i = 0; i < address.Length; i++)
            {
                if (!string.IsNullOrEmpty(address[i]))
                {
                    if (address[i] != PlayerPrefs.GetString("address"))
                    {
                        GameObject t = Instantiate(exButton, new Vector3(), Quaternion.identity);
                        t.GetComponent<RectTransform>().SetParent(parents);
                        t.GetComponentInChildren<Text>().fontSize = 40;
                        t.GetComponentInChildren<Text>().font = label.GetComponent<Text>().font;
                        t.GetComponentInChildren<Text>().color = Color.black;
                        t.GetComponentInChildren<Text>().text = address[i];
                        t.gameObject.SetActive(true);
                    }

                }

            }
        }
        else
        {
            Debug.Log("No Address");
        }
    }


    public void GetAllAddressFromComingMessage()
    {
        Account.Address address = new Account.Address();
        address.address = PlayerPrefs.GetString("address");
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "get_address_coming_message");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonConvert.SerializeObject(address);
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            if (PlayerPrefs.HasKey("AddressList"))
            {
                var result = streamReader.ReadToEnd();
                var comingAddress = JsonConvert.DeserializeObject<Account.AddressList>(result);
                var allAddressList = PlayerPrefs.GetString("AddressList");
                string[] splitAddress = allAddressList.Split(',');
                var isSame = false;
                if (comingAddress.addresslist.Count > 0)
                {
                    for (int i = 0; i < comingAddress.addresslist.Count; i++)
                    {
                        isSame = false;
                        for (int j = 0; j < splitAddress.Length; j++)
                        {
                            if (comingAddress.addresslist[i] == splitAddress[j])
                                isSame = true;
                        }

                        if (!isSame)
                            allAddressList += comingAddress.addresslist[i] + ",";

                    }

                    PlayerPrefs.SetString("AddressList", allAddressList);
                }
            }
            else
            {
                var result = streamReader.ReadToEnd();
                var comingAddress = JsonConvert.DeserializeObject<Account.AddressList>(result);
                string resultAddress = "";
                if (comingAddress.addresslist.Count > 0)
                {
                    for (int i = 0; i < comingAddress.addresslist.Count; i++)
                    {
                        resultAddress += comingAddress.addresslist[i] + ",";
                    }

                    PlayerPrefs.SetString("AddressList", resultAddress);
                }
            }
        }


    }




    public void MineBlock()
    {
        Account.Address address = new Account.Address();
        address.address = PlayerPrefs.GetString("address");
        var isValid = ControlAddress(address.address);
        if (isValid && ControlMineTimeFromAddress(address))
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "mine_block");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(address);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Account.MineBlock>(result);

                Debug.Log(data.transactions[0].amount);
                float pro;

                if (float.TryParse(data.transactions[0].amount, out pro))
                {
                    Debug.Log("You Earn " + pro + " Pro.");
                }



                StartCoroutine(ShowMineMessage(data.message));
                GetAllProCoinFromAddress(address.address,proCoinText);


            }
        }
        else
        {
            Debug.Log("You need to wait 5 hour!");
            StartCoroutine(ShowIssueMineMessage("You must wait 5 hour for mine block!"));
        }

    }


    private bool ControlMineTimeFromAddress(Account.Address address)
    {
        bool isAccessMine = false;
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "get_last_mining_time_from_address");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = JsonConvert.SerializeObject(address);
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
            var data = JsonConvert.DeserializeObject<Account.MineDateTime>(result);
            if (data.timestamp.Equals("-1"))
            {
                isAccessMine = true;
            }
            else
            {
                var mineTime = DateTime.Parse(data.timestamp);
                var resultTime = DateTime.Now - mineTime;
                if(resultTime.Days > 0)
                {
                    isAccessMine = true;
                }
                else
                {
                    if(resultTime.Hours >= 5)
                    {
                        isAccessMine = true;
                    }
                }
            }
        }

        return isAccessMine;
    }



    private IEnumerator ShowMineMessage(string message)
    {
        mineMessage.gameObject.SetActive(true);
        mineMessage.color = Color.green;
        mineMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
        yield return new WaitForSeconds(1.5f);
        mineMessage.gameObject.SetActive(false);

    }

    private IEnumerator ShowIssueMineMessage(string message)
    {
        mineMessage.gameObject.SetActive(true);
        mineMessage.color = Color.red;
        mineMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
        yield return new WaitForSeconds(1.5f);
        mineMessage.gameObject.SetActive(false);

    }



    public void GetAllProCoinFromAddress(string address,TextMeshProUGUI proText)
    {
        Account.Address ad = new Account.Address();
        var isValid = ControlAddress(address);
        if (isValid)
        {
            ad.address = address;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "get_balance");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(ad);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Account.Balance>(result);
                Debug.Log(data.balance);
                proText.text = data.balance + " Pro";


            }
        }
    }


    public void WalletButton()
    {
        mainScreen.gameObject.SetActive(false);
        walletScreen.gameObject.SetActive(true);
        Generate_qr_code generate_Qr_ = new Generate_qr_code();
        qr_image.texture = generate_Qr_.generate_qr_code(PlayerPrefs.GetString("address"));
        var address_text = GameObject.Find("Addres Text");
        address_text.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("address");

    }

    public void BackButtonFromTransactionScreen()
    {
        transactionScreen.gameObject.SetActive(false);
        walletScreen.gameObject.SetActive(true);
    }


    public void SendProButton()
    {
        transactionScreen.gameObject.SetActive(true);
        walletScreen.gameObject.SetActive(false);
    }


    public void SendPro()
    {
        GetAllProCoinFromAddress(PlayerPrefs.GetString("address"),proCoinText);
        string[] takeCoin = proCoinText.text.Split(' ');
        float walletAmount;
        float.TryParse(takeCoin[0], out walletAmount);

        var otherAddressText = GameObject.Find("Other Address");
        var amountText = GameObject.Find("Amount");
        if (!string.IsNullOrEmpty(otherAddressText.GetComponent<TMP_InputField>().text))
        {
            var isValid = ControlAddress(otherAddressText.GetComponent<TMP_InputField>().text);
            if (isValid && otherAddressText.GetComponent<TMP_InputField>().text != PlayerPrefs.GetString("address"))
            {
                if (!string.IsNullOrEmpty(amountText.GetComponent<TMP_InputField>().text))
                {
                    float sendingAmount;
                    if (float.TryParse(amountText.GetComponent<TMP_InputField>().text, out sendingAmount))
                    {
                        //float.TryParse(amountText.GetComponent<TMP_InputField>().text, out sendingAmount);
                        if (sendingAmount <= walletAmount && sendingAmount > 0)
                        {
                            Debug.Log("Success sending!");
                            string amount = "";
                            amount = "f0b6081f8b6f472b8d27ca04537e50c4d2a17d6f05fe466db0fbf89cfe1e51f0,";
                            amount += sendingAmount.ToString();
                            StartCoroutine(ShowTransactionSuccess("Transaction Success"));
                            SendTransactionPro(PlayerPrefs.GetString("address"), otherAddressText.GetComponent<TMP_InputField>().text, amount,proCoinText);
                        }
                        else
                        {
                            StartCoroutine(ShowTransactionIssue("You can not enough Pro!"));
                        }
                    }
                    else
                    {
                        StartCoroutine(ShowTransactionIssue("Amount Issue!"));

                    }

                }
            }
            else
            {
                StartCoroutine(ShowTransactionIssue("Address is not valid!"));
            }
        }

    }

    private IEnumerator ShowTransactionIssue(string message)
    {
        transactionIssueMessage.gameObject.SetActive(true);
        transactionIssueMessage.color = Color.red;
        transactionIssueMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
        yield return new WaitForSeconds(1.5f);
        transactionIssueMessage.gameObject.SetActive(false);

    }

    private IEnumerator ShowTransactionSuccess(string message)
    {
        transactionIssueMessage.gameObject.SetActive(true);
        transactionIssueMessage.color = Color.green;
        transactionIssueMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
        yield return new WaitForSeconds(1.5f);
        transactionIssueMessage.gameObject.SetActive(false);
    }

    public void ScanQRButton()
    {
        scanQRObject.gameObject.SetActive(true);
    }

    public void CloseScanQR()
    {
        scanQRObject.gameObject.SetActive(false);
    }

    public void CreatePaymentQRCode()
    {
        Generate_qr_code generateQR = new Generate_qr_code();
        if (!string.IsNullOrEmpty(createPaymentInputField.text))
        {
            float pro;
            if(float.TryParse(createPaymentInputField.text,out pro))
            {
                if(pro > 0)
                {
                    string data = PlayerPrefs.GetString("address") + "," + pro.ToString();
                    var qr = generateQR.generate_qr_code(data);
                    qr_image2.texture = qr;
                }
            }
        }
    }

    public void CreatePaymentButton()
    {
        mainScreen.gameObject.SetActive(false);
        createPaymentScreen.gameObject.SetActive(true);
    }



    // Update is called once per frame
    void Update()
    {

    }
}
