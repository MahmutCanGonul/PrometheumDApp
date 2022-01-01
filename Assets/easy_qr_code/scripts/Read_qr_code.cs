using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using epoching.easy_debug_on_the_phone;
using UnityEngine.UI;
using epoching.easy_gui;
using TMPro;

namespace epoching.easy_qr_code
{
    public class Read_qr_code : MonoBehaviour
    {
        [Header("raw_image_video")]
        public RawImage raw_image_video;

        [Header("audio source")]
        //public AudioSource audio_source;

        //camera texture
        private WebCamTexture cam_texture;

        //is reading qr_code
        private bool is_reading = false;


        public GameObject scanQRObject;
        public GameObject transactionScreen;
        public GameObject walletScreen;
        public TextMeshProUGUI proText;
        public Image warningPayment;


        void OnEnable()
        {
            StartCoroutine(this.start_webcam());
        }

        private IEnumerator start_webcam()
        {
            yield return new WaitForSeconds(0.11f);

            //init camera texture
            this.cam_texture = new WebCamTexture();

            //this.cam_texture.requestedWidth = 720;
            //this.cam_texture.requestedHeight = 1280;

            this.cam_texture.requestedWidth = 540;
            this.cam_texture.requestedHeight = 720;


            this.cam_texture.Play();

            if (Application.platform == RuntimePlatform.Android)
            {
                this.raw_image_video.rectTransform.sizeDelta = new Vector2(Screen.width * cam_texture.width / (float)this.cam_texture.height, Screen.width);
                this.raw_image_video.rectTransform.rotation = Quaternion.Euler(0, 0, -90);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                this.raw_image_video.rectTransform.sizeDelta = new Vector2(1080, 1080 * this.cam_texture.width / (float)this.cam_texture.height);
                this.raw_image_video.rectTransform.localScale = new Vector3(-1, 1, 1);
                this.raw_image_video.rectTransform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                this.raw_image_video.rectTransform.sizeDelta = new Vector2(Camera.main.pixelWidth, Camera.main.pixelWidth * this.cam_texture.height / (float)this.cam_texture.width);
                this.raw_image_video.rectTransform.localScale = new Vector3(-1, 1, 1);
            }

            this.raw_image_video.texture = cam_texture;

            this.is_reading = true;

            yield return null;
        }


        void OnDisable()
        {
            if (this.cam_texture != null)
            {
                this.cam_texture.Stop();
            }
        }

        private float interval_time = 0.1f;
        private float time_stamp = 0;

        private IEnumerator SuccessPayment()
        {
            warningPayment.gameObject.SetActive(true);
            warningPayment.GetComponentInChildren<TextMeshProUGUI>().text = "Payment Success";
            warningPayment.color = Color.green;
            yield return new WaitForSeconds(1.5f);
            warningPayment.gameObject.SetActive(false);

        }

        private IEnumerator DenisedPayment()
        {
            warningPayment.gameObject.SetActive(true);
            warningPayment.GetComponentInChildren<TextMeshProUGUI>().text = "Payment Denised";
            warningPayment.color = Color.red;
            yield return new WaitForSeconds(1.5f);
            warningPayment.gameObject.SetActive(false);
        }

        void Update()
        {
            if (this.is_reading)
            {
                this.time_stamp += Time.deltaTime;

                if (this.time_stamp > this.interval_time)
                {
                    this.time_stamp = 0;

                    try
                    {
                        Debug.Log("reading");

                        IBarcodeReader barcodeReader = new BarcodeReader();

                        if (this.cam_texture != null && this.cam_texture.isPlaying && this.cam_texture.isReadable)
                        {
                            // decode the current frame
                            var result = barcodeReader.Decode(this.cam_texture.GetPixels32(), this.cam_texture.width, this.cam_texture.height);
                            Prometheum pro = new Prometheum();

                            if (result != null && pro.ControlAddress(result.Text))
                            {
                                Canvas_confirm_box.confirm_box
                                (
                                    "Detect Address",
                                    result.Text,
                                    "cancel",
                                    "JUMP",
                                     delegate ()
                                     {
                                         this.is_reading = true;
                                     },
                                     delegate ()
                                     {
                                         Debug.Log("jump to link");
                                         //Application.OpenURL(result.Text);
                                         scanQRObject.gameObject.SetActive(false);
                                         walletScreen.gameObject.SetActive(false);
                                         transactionScreen.gameObject.SetActive(true);
                                         var otherAddress = GameObject.Find("Other Address");
                                         otherAddress.GetComponent<TMP_InputField>().text = result.Text;
                                         this.is_reading = true;
                                     }
                                );
                                Debug.Log("DECODED TEXT FROM QR: " + result.Text);

                                this.is_reading = false;

                                //this.audio_source.Play();
                            }
                            else
                            {
                                string[] splitData = result.Text.Split(',');
                                if (splitData.Length == 2)
                                {
                                    float amount;
                                    if (pro.ControlAddress(splitData[0]) && float.TryParse(splitData[1], out amount))
                                    {
                                        //Access the payment

                                        Canvas_confirm_box.confirm_box
                                     (
                                         "Detect Payment",
                                         "Address: "+splitData[0]+"&& Pro: "+splitData[1],
                                         "CANCEL",
                                         "PAY",
                                          delegate ()
                                          {
                                              this.is_reading = true;
                                          },
                                          delegate ()
                                          {
                                              //If you click pay
                                              string[] wallet = proText.text.Split(' ');
                                              float walletAmount=0;
                                              float needAmount = 0;
                                              float.TryParse(wallet[0], out walletAmount);
                                              float.TryParse(splitData[1], out walletAmount);
                                              var proData = "f0b6081f8b6f472b8d27ca04537e50c4d2a17d6f05fe466db0fbf89cfe1e51f0,";
                                              proData += splitData[1];
                                              if (walletAmount >= needAmount)
                                              {
                                                  //Payment Success
                                                  pro.SendTransactionPro(PlayerPrefs.GetString("address"),splitData[0],proData,proText);
                                                  //pro.GetAllProCoinFromAddress(PlayerPrefs.GetString("address"),proText);
                                                  StartCoroutine(SuccessPayment());
                                              }
                                              else
                                              {
                                                  StartCoroutine(DenisedPayment());
                                              }
                                              this.is_reading = true;
                                          }
                                     );


                                        this.is_reading = false;

                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex.Message);
                        //Canvas_confirm_box.confirm_box
                        //(
                        //    "confirm box",
                        //    "error>>>" + ex.Message,
                        //    "cancel",
                        //    "OK",
                        //    true,
                        //    delegate ()
                        //    {
                        //        this.is_reading = true;
                        //    },
                        //    delegate ()
                        //    {
                        //        this.is_reading = true;
                        //    }
                        //);
                        //this.is_reading = false;
                    }
                }
            }
        }
    }
}

