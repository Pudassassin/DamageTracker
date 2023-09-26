using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UnboundLib;
using System.Globalization;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using HarmonyLib;

namespace DamageTracker.MonoBehaviors
{
    public class PlayerDamageTracker : MonoBehaviour
    {
        public static GameObject prefabNumberObject = DamageTracker.MainAsset.LoadAsset<GameObject>("DamageNumber");
        public static GameObject prefabNumberCanvas = DamageTracker.MainAsset.LoadAsset<GameObject>("DamageNumberCanvas");

        public enum NumberFormatting
        {
            none,
            spacing,
            writtenNum,
            shortenNum,
            metric,
            sciencific
        }
        public static List<string> postfixIRL = new List<string>()
        {
            "ki",       // 10^3
            "mil",      // 10^6
            "bil",      // 10^9
            "tri",      // 10^12 and so on
            "quad",
            "quin",
            "sext",
            "septi",
            "octil",
            "nonil",
            "decill"
        };
        public static List<string> postfixMetric = new List<string>()
        {
            "ki",
            "Me",
            "Gi",
            "Te",
            "Pe",
            "Ex",
            "Ze",
            "Yo"
        };

        // !! // Setting variables
        public static NumberFormatting formatSetting = NumberFormatting.spacing;
        public static bool usePlayerColor = true;

        public static float maxValueToShowDecimal = 25.0f;
        public static bool hideDecimalOveride = false;
        public static float scaleDecimalPart = 0.75f;

        // public static bool trackUnspecifiedHealDelta = true;

        public static float baseFontSize = 24f;
        public static float baseNumberScale = 1.0f;

        // scaling scale is mul by log of value then add to base scale
        public static float logNumberScale = 0.25f;
        public static float logExponent = 100.0f;

        public static float damageNumberLifetime = 2.25f;
        public static float damageNumberFullOpaqueTime = 1.45f;

        public static float dpsMeasureWindow = 0.995f;
        public static float dpsMeasureIdleTime = 0.35f;

        public static Vector3 numberSpawnOffset     = new Vector3(0.0f, 35.0f, 0.0f);
        public static Vector3 numberSpawnOffsetHeal = new Vector3(-10.0f, 5.0f, 0.0f);
        public static Vector3 numberObjectVelocity  = new Vector3(0.0f, 35.0f, 0.0f);

        public static Color colorDamage             = new Color(1f, .35f, .35f);
        public static Color colorHeal               = new Color(.35f, 1f, .35f);
        public static Color colorNegHeal            = new Color(1f, .35f, 1f);
        public static Color colorUnclassified       = new Color(.75f, .75f, .75f);

        public static Color brightenPlayerColor     = new Color(.20f, .20f, .20f, 1.0f);
        public static Color darkenNumberOutline     = new Color(.50f, .50f, .50f, 0.45f);

        // !! // Font attribute
        public static float fontWidthToHeightRatio = 0.5f;
        public static float textPadding = 15.0f;
        public static float textUnstackingSpeedMul = 6.5f;

        // !! // Instance variables / data
        public static GameObject canvasObject = null;
        public static List<GameObject> numberObjects = new List<GameObject>();

        private GameObject focusNumDamage, focusNumHeal, focusNumNegHeal, focusNumMisc;
        private float timerNumDamage, timerNumHeal, timerNumNegHeal, timerNumMisc;
        private float timerLastDamage, timerLastHeal, timerLastNegHeal, timerLastMisc;

        // private float prevHealth = 0.0f, sumDamage = 0.0f, sumHeal = 0.0f, sumNegHeal = 0.0f;
        // private float timerLastDelta = 0.0f;

        private CharacterData playerData;
        private Color playerColor = Color.black;

        public static void UpdateConfigs()
        {
            formatSetting           = (NumberFormatting)DamageTracker.NumberFormattingStyle;
            usePlayerColor          = DamageTracker.UsePlayerColor;

            maxValueToShowDecimal   = DamageTracker.MaxNumberToShowDecimal;
            scaleDecimalPart        = DamageTracker.ScaleDecimalTextSize;
            hideDecimalOveride      = DamageTracker.HideAllDecimal;

            baseFontSize            = DamageTracker.NumberFontSize;
            logNumberScale          = DamageTracker.ScaleNumberSizeByValue;

            damageNumberLifetime    = DamageTracker.NumberLifetime;
            damageNumberFullOpaqueTime = DamageTracker.NumberOpaqueTime;
            dpsMeasureWindow        = DamageTracker.NumberSumTimeWindow;
            dpsMeasureIdleTime      = DamageTracker.NumberNewSumDelay;
        }

        // !! // MonoBehavior
        public void Awake()
        {
            playerData = GetComponent<CharacterData>();

            PlayerSkinParticle skinParticle = GetComponentInChildren<PlayerSkinParticle>();
            playerColor = Traverse.Create(skinParticle).Field("startColor2").GetValue<Color>();

            // prevHealth = playerData.health;

            // instantiate canvas object
            if (canvasObject == null)
            {
                canvasObject = GameObject.Instantiate(prefabNumberCanvas);
            }
        }

        public void Update()
        {
            if (playerData == null)
            {
                enabled = false;
                return;
            }

            if (focusNumDamage != null)
            {
                if (timerNumDamage > dpsMeasureWindow || timerLastDamage > dpsMeasureIdleTime)
                {
                    focusNumDamage = null;
                    timerNumDamage = 0.0f;
                    timerLastDamage = 0.0f;
                }
                else
                {
                    timerLastDamage += TimeHandler.deltaTime;
                    timerNumDamage += TimeHandler.deltaTime;
                }
            }

            if (focusNumHeal != null)
            {
                if (timerNumHeal > dpsMeasureWindow || timerLastHeal > dpsMeasureIdleTime)
                {
                    focusNumHeal = null;
                    timerNumHeal = 0.0f;
                    timerLastHeal = 0.0f;
                }
                else
                {
                    timerLastHeal += TimeHandler.deltaTime;
                    timerNumHeal += TimeHandler.deltaTime;
                }
            }

            if (focusNumNegHeal != null)
            {
                if (timerNumNegHeal > dpsMeasureWindow || timerLastNegHeal > dpsMeasureIdleTime)
                {
                    focusNumNegHeal = null;
                    timerNumNegHeal = 0.0f;
                    timerLastNegHeal = 0.0f;
                }
                else
                {
                    timerLastNegHeal += TimeHandler.deltaTime;
                    timerNumNegHeal += TimeHandler.deltaTime;
                }
            }

            // NYI : the change that is not by damages or heals
            // if (focusNumMisc != null)
            // {
            //     if (timerNumMisc > dpsMeasureWindow || timerLastMisc > dpsMeasureIdleTime)
            //     {
            //         focusNumMisc = null;
            //         timerNumMisc = 0.0f;
            //         timerLastMisc = 0.0f;
            //     }
            //     else
            //     {
            //         timerLastMisc += TimeHandler.deltaTime;
            //         timerNumMisc += TimeHandler.deltaTime;
            //     }
            // }

        }

        public void TrackDamage(float number)
        {
            if (number < 0.0f) { return; }
            if (!ModdingUtils.Utils.PlayerStatus.PlayerAliveAndSimulated(playerData.player) || playerData.healthHandler.isRespawning)
            {
                return;
            }

            DamageNumberMono numberMono = null;

            if (focusNumDamage != null)
            {
                numberMono = focusNumDamage.GetComponent<DamageNumberMono>();

                timerLastDamage = 0.0f;
            }
            else
            {
                focusNumDamage = GameObject.Instantiate(prefabNumberObject, canvasObject.transform);
                Vector3 playerScreenPos = MainCam.instance.cam.WorldToScreenPoint(transform.position);
                focusNumDamage.transform.position = playerScreenPos + numberSpawnOffset;

                numberMono = focusNumDamage.AddComponent<DamageNumberMono>();

                numberMono.type = DamageNumberMono.NumberType.damage;
                if (usePlayerColor)
                {
                    numberMono.textColor = playerColor + brightenPlayerColor;
                    numberMono.outlineColor = colorDamage - darkenNumberOutline;
                }
                else
                {
                    numberMono.textColor = colorDamage;
                    numberMono.outlineColor = Color.black;
                }

                numberMono.lifetime = damageNumberLifetime;
                numberMono.timeFullOpaque = damageNumberFullOpaqueTime;
                numberMono.velocity = numberObjectVelocity;

                numberMono.GetComponent<Text>().text = "";
            }

            // sumDamage += number;

            numberMono.value += number;
            numberMono.display = FormatNumber(numberMono.value, formatSetting);

            //numberMono.fontSize = baseFontSize * (baseNumberScale + (Mathf.Log(numberMono.value, logExponent) - 1.0f) * logNumberScale);
            numberMono.fontSize = ScaleFontSizeByValue(numberMono.value);
            numberMono.display = ResizeDecimals(numberMono.display, numberMono.fontSize);

            if (usePlayerColor)
            {
                numberMono.display = "- " + numberMono.display;
            }
        }

        public void TrackHeal(float number)
        {
            if (number < 0.0f) { return; }
            if (!ModdingUtils.Utils.PlayerStatus.PlayerAliveAndSimulated(playerData.player) || playerData.healthHandler.isRespawning)
            {
                return;
            }

            DamageNumberMono numberMono = null;

            if (focusNumHeal != null)
            {
                numberMono = focusNumHeal.GetComponent<DamageNumberMono>();

                timerLastHeal = 0.0f;
            }
            else
            {
                focusNumHeal = GameObject.Instantiate(prefabNumberObject, canvasObject.transform);
                Vector3 playerScreenPos = MainCam.instance.cam.WorldToScreenPoint(transform.position);
                focusNumHeal.transform.position = playerScreenPos + numberSpawnOffset + numberSpawnOffsetHeal;

                numberMono = focusNumHeal.AddComponent<DamageNumberMono>();

                numberMono.type = DamageNumberMono.NumberType.heal;
                if (usePlayerColor)
                {
                    numberMono.textColor = playerColor + brightenPlayerColor;
                    numberMono.outlineColor = colorHeal - darkenNumberOutline;
                }
                else
                {
                    numberMono.textColor = colorHeal;
                    numberMono.outlineColor = Color.black;
                }

                numberMono.lifetime = damageNumberLifetime;
                numberMono.timeFullOpaque = damageNumberFullOpaqueTime;
                numberMono.velocity = numberObjectVelocity;

                numberMono.GetComponent<Text>().text = "";
            }

            // sumHeal += number;
            // sumHeal += Mathf.Min(number, playerData.maxHealth - playerData.health);

            numberMono.value += number;
            numberMono.display = FormatNumber(numberMono.value, formatSetting);

            // numberMono.fontSize = baseFontSize * (baseNumberScale + (Mathf.Log(numberMono.value, logExponent) - 1.0f) * logNumberScale);
            numberMono.fontSize = ScaleFontSizeByValue(numberMono.value);
            numberMono.display = ResizeDecimals(numberMono.display, numberMono.fontSize);

            if (usePlayerColor)
            {
                numberMono.display = "+ " + numberMono.display;
            }
        }

        public void TrackNegHeal(float number)
        {
            if (number < 0.0f) { return; }
            if (!ModdingUtils.Utils.PlayerStatus.PlayerAliveAndSimulated(playerData.player) || playerData.healthHandler.isRespawning)
            {
                return;
            }

            DamageNumberMono numberMono = null;

            if (focusNumNegHeal != null)
            {
                numberMono = focusNumNegHeal.GetComponent<DamageNumberMono>();

                timerLastNegHeal = 0.0f;
            }
            else
            {
                focusNumNegHeal = GameObject.Instantiate(prefabNumberObject, canvasObject.transform);
                Vector3 playerScreenPos = MainCam.instance.cam.WorldToScreenPoint(transform.position);
                focusNumNegHeal.transform.position = playerScreenPos + numberSpawnOffset - numberSpawnOffsetHeal;

                numberMono = focusNumNegHeal.AddComponent<DamageNumberMono>();

                numberMono.type = DamageNumberMono.NumberType.negHeal;
                if (usePlayerColor)
                {
                    numberMono.textColor = playerColor + brightenPlayerColor;
                    numberMono.outlineColor = colorNegHeal - darkenNumberOutline;
                }
                else
                {
                    numberMono.textColor = colorNegHeal;
                    numberMono.outlineColor = Color.black;
                }

                numberMono.type = DamageNumberMono.NumberType.negHeal;
                numberMono.lifetime = damageNumberLifetime;
                numberMono.timeFullOpaque = damageNumberFullOpaqueTime;
                numberMono.velocity = numberObjectVelocity;

                numberMono.GetComponent<Text>().text = "";
            }

            // sumNegHeal += number;

            numberMono.value += number;
            numberMono.display = FormatNumber(numberMono.value, formatSetting);

            // numberMono.fontSize = baseFontSize * (baseNumberScale + (Mathf.Log(numberMono.value, logExponent) - 1.0f) * logNumberScale);
            numberMono.fontSize = ScaleFontSizeByValue(numberMono.value);
            numberMono.display = ResizeDecimals(numberMono.display, numberMono.fontSize);

            if (usePlayerColor)
            {
                numberMono.display = "- " + numberMono.display + "*";
            }
        }

        // public void ShowDemo(float number, DamageNumberMono.NumberType numberType)
        // {
        //     DamageNumberMono numberMono = null;
        // 
        //     if (focusNumNegHeal != null)
        //     {
        //         numberMono = focusNumNegHeal.GetComponent<DamageNumberMono>();
        // 
        //         timerLastNegHeal = 0.0f;
        //     }
        //     else
        //     {
        //         focusNumNegHeal = GameObject.Instantiate(prefabNumberObject, canvasObject.transform);
        //         Vector3 playerScreenPos = MainCam.instance.cam.WorldToScreenPoint(transform.position);
        //         focusNumNegHeal.transform.position = playerScreenPos + numberSpawnOffset - numberSpawnOffsetHeal;
        // 
        //         numberMono = focusNumNegHeal.AddComponent<DamageNumberMono>();
        // 
        //         numberMono.type = DamageNumberMono.NumberType.negHeal;
        //         if (usePlayerColor)
        //         {
        //             numberMono.textColor = playerColor + brightenPlayerColor;
        //             numberMono.outlineColor = colorNegHeal - darkenNumberOutline;
        //         }
        //         else
        //         {
        //             numberMono.textColor = colorNegHeal;
        //             numberMono.outlineColor = Color.black;
        //         }
        // 
        //         numberMono.type = DamageNumberMono.NumberType.negHeal;
        //         numberMono.lifetime = damageNumberLifetime;
        //         numberMono.timeFullOpaque = damageNumberFullOpaqueTime;
        //         numberMono.velocity = numberObjectVelocity;
        // 
        //         numberMono.GetComponent<Text>().text = "";
        //     }
        // 
        //     // sumNegHeal += number;
        // 
        //     numberMono.value += number;
        //     numberMono.display = FormatNumber(numberMono.value, formatSetting);
        // 
        //     numberMono.fontSize = baseFontSize * (baseNumberScale + (Mathf.Log(numberMono.value, logExponent) - 1.0f) * logNumberScale);
        //     numberMono.display = ResizeDecimals(numberMono.display, numberMono.fontSize);
        // 
        //     if (usePlayerColor)
        //     {
        //         numberMono.display = "- " + numberMono.display + "*";
        //     }
        // }

        // !! // Methods
        public static string FormatNumber(float number, NumberFormatting formatting)
        {
            //let another method handle type of number
            string resultStr = number.ToString();
            bool showDecimal = (number < maxValueToShowDecimal) && (!hideDecimalOveride);

            switch (formatting)
            {
                case NumberFormatting.none:
                    if (showDecimal)
                    {
                        resultStr = number.ToString("F3");
                    }
                    else
                    {
                        resultStr = number.ToString("F0");
                    }
                    break;

                case NumberFormatting.spacing:
                    if (showDecimal)
                    {
                        resultStr = number.ToString("N3", CultureInfo.InvariantCulture).Replace(',', ' ');
                    }
                    else
                    {
                        resultStr = number.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');
                    }
                    break;

                case NumberFormatting.writtenNum:
                    if (showDecimal)
                    {
                        resultStr = number.ToString("N3");
                    }
                    else
                    {
                        resultStr = number.ToString("N0");
                    }
                    break;

                case NumberFormatting.shortenNum:
                    resultStr = FormatColloquialNumber(number);
                    break;

                case NumberFormatting.metric:
                    resultStr = FormatMetricNumber(number);
                    break;

                case NumberFormatting.sciencific:
                    if (number <= 1000000.0f)
                    {
                        if (showDecimal)
                        {
                            resultStr = number.ToString("N3", CultureInfo.InvariantCulture).Replace(',', ' ');
                        }
                        else
                        {
                            resultStr = number.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');
                        }
                    }
                    else
                    {
                        resultStr = number.ToString("E2");
                    }
                    break;
            }

            return resultStr;
        }

        public static void ExtractFloat(float number, out int exponent, out float mantissa, bool divThousand = true)
        {
            // only positive exponent; eg. value of 1 and upward
            exponent = 0;

            if (divThousand)
            {
                while (number > 1000.0f)
                {
                    number /= 1000.0f;
                    exponent += 3;
                }
            }
            else
            {
                while (number > 10.0f)
                {
                    number /= 10.0f;
                    exponent++;
                }
            }

            mantissa = number;
        }

        public static string FormatColloquialNumber(float number)
        {
            string resultStr = number.ToString("N3");
            bool showDecimal = (number < maxValueToShowDecimal) && (!hideDecimalOveride);

            if (number >= 1000.0f)
            {
                float mant;
                int expo;
                ExtractFloat(number, out expo, out mant);

                int postfixIndex = (expo / 3) - 1;
                if (postfixIndex >= postfixIRL.Count)
                {
                    postfixIndex = postfixIRL.Count - 1;

                    int tempExpo = (postfixIndex + 1) * 3;
                    mant = number / Mathf.Pow(10.0f, tempExpo);
                }

                if (!hideDecimalOveride)
                {
                    resultStr = mant.ToString("N2") + " " + postfixIRL[postfixIndex];
                }
                else
                {
                    resultStr = mant.ToString("N0") + " " + postfixIRL[postfixIndex];
                }
            }
            else
            {
                if (showDecimal)
                {
                    resultStr = number.ToString("N3");
                }
                else
                {
                    resultStr = number.ToString("N0");
                }
            }

            return resultStr;
        }

        public static string FormatMetricNumber(float number)
        {
            string resultStr = number.ToString("F3");
            bool showDecimal = (number < maxValueToShowDecimal) && (!hideDecimalOveride);

            if (number >= 1000.0f)
            {
                float mant;
                int expo;
                ExtractFloat(number, out expo, out mant);

                int postfixIndex = (expo / 3) - 1;
                if (postfixIndex >= postfixMetric.Count)
                {
                    postfixIndex = postfixMetric.Count - 1;

                    int tempExpo = (postfixIndex + 1) * 3;
                    mant = number / Mathf.Pow(10.0f, tempExpo);
                }

                if (!hideDecimalOveride)
                {
                    resultStr = mant.ToString("F3") + " " + postfixMetric[postfixIndex];
                }
                else
                {
                    resultStr = mant.ToString("F0") + " " + postfixMetric[postfixIndex];
                }
            }
            else
            {
                if (showDecimal)
                {
                    resultStr = number.ToString("F3");
                }
                else
                {
                    resultStr = number.ToString("F0");
                }
            }

            return resultStr;
        }

        public static float ScaleFontSizeByValue(float value)
        {
            return baseFontSize * (baseNumberScale + (Mathf.Log(value, logExponent) - 1.0f) * logNumberScale);
        }

        public static string ResizeDecimals(string input, float currentSize)
        {
            string resultStr = input;
            int scaledSize = Mathf.FloorToInt(currentSize * scaleDecimalPart);

            if (resultStr.Contains("."))
            {
                int decimalStartI = resultStr.IndexOf(".") + 1;

                resultStr = resultStr.Insert(decimalStartI, $"<size={scaledSize}>");

                int decimalEndI = Mathf.Max(resultStr.IndexOf(" ", decimalStartI), resultStr.IndexOf("E+", decimalStartI), resultStr.IndexOf("e+", decimalStartI));

                if (decimalEndI >= 0)
                {
                    resultStr = resultStr.Insert(decimalEndI, "</size>");
                }
                else
                {
                    resultStr += "</size>";
                }
            }

            return resultStr;
        }
    }

    public class DamageNumberMono : MonoBehaviour
    {
        public enum NumberType
        {
            unclassified,
            damage,
            heal,
            negHeal
        }

        // public GameObject gameObject;
        public NumberType type;

        public Text text;
        public Outline textOutline;

        public float value;
        public string display;
        public float fontSize;

        public float lifetime = 3.0f, timeFullOpaque = 1.5f;
        public float timer = 0.0f;

        public Vector2 textSize = Vector2.zero;

        public Vector3 velocity = new Vector3(0.0f, 1.5f, 0.0f);
        public bool isUnstacking = false;

        public Color textColor = Color.white;
        public Color outlineColor = Color.black;

        public void Awake()
        {
            text = GetComponent<Text>();
            textOutline = GetComponent<Outline>();

            // UpdateText();

            PlayerDamageTracker.numberObjects.Add(gameObject);
        }

        public void Update()
        {
            UpdateText();

            if (timer > lifetime)
            {
                PlayerDamageTracker.numberObjects.Remove(gameObject);
                Destroy(gameObject);
            }
            else if (timer > timeFullOpaque)
            {
                float alpha = 1.0f - ((timer - timeFullOpaque) / (lifetime - timeFullOpaque));
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                textOutline.effectColor = new Color(textOutline.effectColor.r, textOutline.effectColor.g, textOutline.effectColor.b, alpha);
            }

            transform.localPosition += velocity * TimeHandler.deltaTime;

            isUnstacking = false;
            foreach (GameObject numberObj in PlayerDamageTracker.numberObjects)
            { 
                DamageNumberMono numberMono = numberObj.GetComponent<DamageNumberMono>();
                if (numberMono == null) { continue; }
                else if (numberObj == gameObject) { continue; }

                if (numberObj.transform.position.y <= transform.position.y)
                {
                    float deltaX = Mathf.Abs(numberObj.transform.position.x - transform.position.x);
                    float deltaY = Mathf.Abs(numberObj.transform.position.y - transform.position.y);

                    if (deltaY < textSize.y / 2.0f && deltaX < textSize.x / 2.0f)
                    {
                        isUnstacking = true;
                        break;
                    }
                }
            }

            if (isUnstacking)
            {
                transform.localPosition += velocity * TimeHandler.deltaTime * PlayerDamageTracker.textUnstackingSpeedMul;
            }

            timer += TimeHandler.deltaTime;
        }

        private void UpdateText()
        {
            text.color = textColor;
            textOutline.effectColor = outlineColor;

            text.text = display;
            text.fontSize = Mathf.CeilToInt(fontSize);

            // size is the box covering entire text + paddings
            textSize.y = fontSize + (PlayerDamageTracker.textPadding * 2.0f);
            textSize.x = (fontSize * PlayerDamageTracker.fontWidthToHeightRatio * display.Length) + (PlayerDamageTracker.textPadding * 2.0f);
        }
    }
}
