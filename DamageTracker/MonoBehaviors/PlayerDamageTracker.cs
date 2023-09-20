using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using UnboundLib;
using System.Globalization;
using UnityEngine.UI;

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
        public static float maxValueToShowDecimal = 25.0f;
        public static bool hideDecimalOveride = false;

        public static bool trackUnspecifiedHealDelta = true;

        public static float baseFontSize = 12f;
        public static float baseNumberScale = 1.0f;

        // scaling scale is mul by log of value then add to base scale
        public static float logNumberScale = 0.25f;
        public static float logExponent = 100.0f;

        public static float damageNumberLifetime = 2.0f;
        public static float damageNumberFullOpaqueTime = 1.5f;

        public static float dpsMeasureWindow = 1.0f;
        public static float dpsMeasureIdleTime = 0.35f;

        public static Vector3 numberSpawnOffset = new Vector3(0.0f, 12.5f, 0.0f);
        public static Vector3 numberObjectVelocity    = new Vector3(0.0f, 35.0f, 0.0f);

        public static Color colorDamage         = new Color(1f, .35f, .35f);
        public static Color colorHeal           = new Color(.35f, 1f, .35f);
        public static Color colorNegHeal        = new Color(1f, .35f, 1f);
        public static Color colorUnclassified   = new Color(.75f, .75f, .75f);

        // !! // Instance variables / data
        public static GameObject canvasObject = null;
        private List<GameObject> numberObjects = new List<GameObject>();
        private GameObject focusNumDamage, focusNumHeal, focusNumNegHeal, focusNumMisc;
        private float timerNumDamage, timerNumHeal, timerNumNegHeal, timerNumMisc;
        private float timerLastDamage, timerLastHeal, timerLastNegHeal, timerLastMisc;

        private float prevHealth = 0.0f, sumDamage = 0.0f, sumHeal = 0.0f, sumNegHeal = 0.0f;
        private float timerLastDelta = 0.0f;

        private CharacterData playerData;

        // !! // MonoBehavior
        public void Awake()
        {
            playerData = GetComponent<CharacterData>();
            prevHealth = playerData.health;

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

            // to be implemented uniquely
            if (focusNumMisc != null)
            {
                if (timerNumMisc > dpsMeasureWindow || timerLastMisc > dpsMeasureIdleTime)
                {
                    focusNumMisc = null;
                    timerNumMisc = 0.0f;
                    timerLastMisc = 0.0f;
                }
                else
                {
                    timerLastMisc += TimeHandler.deltaTime;
                    timerNumMisc += TimeHandler.deltaTime;
                }
            }

        }

        public void TrackDamage(float number)
        {
            if (number < 0.0f) { return; }

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
                numberMono.lifetime = damageNumberLifetime;
                numberMono.timeFullOpaque = damageNumberFullOpaqueTime;
                numberMono.velocity = numberObjectVelocity;

                numberMono.GetComponent<Text>().text = "";
            }

            sumDamage += number;

            numberMono.value += number;
            numberMono.display = FormatNumber(numberMono.value, formatSetting);
            numberMono.fontSize = baseFontSize * (baseNumberScale + (Mathf.Log(numberMono.value, logExponent) - 1.0f) * logNumberScale);
            // numberMono.transform.localScale = Vector3.one * (baseNumberScale + (Mathf.Log(number, 10.0f) - 1.0f) * logNumberScale);
        }

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
                    FormatColloquialNumber(number);
                    break;

                case NumberFormatting.metric:
                    FormatMetricNumber(number);
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
                        resultStr = number.ToString("e2");
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
        public float value;
        public string display;
        public float fontSize;

        public float lifetime = 3.0f, timeFullOpaque = 1.5f;
        public float timer = 0.0f;

        public Vector3 velocity = new Vector3(0.0f, 1.5f, 0.0f);

        public void Awake()
        {
            text = GetComponent<Text>();
        }

        public void Update()
        {
            switch (type)
            {
                case NumberType.unclassified:
                    text.color = PlayerDamageTracker.colorUnclassified;
                    break;
                case NumberType.damage:
                    text.color = PlayerDamageTracker.colorDamage;
                    break;
                case NumberType.heal:
                    text.color = PlayerDamageTracker.colorHeal;
                    break;
                case NumberType.negHeal:
                    text.color = PlayerDamageTracker.colorNegHeal;
                    break;
                default:
                    text.color = PlayerDamageTracker.colorUnclassified;
                    break;
            }

            if (timer > lifetime)
            {
                Destroy(gameObject);
            }
            else if (timer > timeFullOpaque)
            {
                float alpha = 1.0f - ((timer - timeFullOpaque) / (lifetime - timeFullOpaque));
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            }

            text.text = display;
            text.fontSize = Mathf.CeilToInt(fontSize);

            transform.localPosition += velocity * TimeHandler.deltaTime;

            timer += TimeHandler.deltaTime;
        }
    }
}
