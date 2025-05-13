using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MetricsMenu : UIStack.Context
{
    [SerializeField] private TMPro.TextMeshProUGUI firstMetricTextObject;
    [SerializeField] private Button backButton;
    [SerializeField] private Button resetMetricsButton;
    [SerializeField] private string[] metricsToAlwaysDisplayEvenIfUnset;
    [SerializeField] private string[] metricsCountedInSeconds;

    private struct MetricTracker
    {
        public string key;
        public bool countedInSeconds;
        public TMPro.TextMeshProUGUI textObject;
        public MetricTracker(
            string key,
            bool countedInSeconds,
            TMPro.TextMeshProUGUI textObject
        ) {
            this.key = key;
            this.countedInSeconds = countedInSeconds;
            this.textObject = textObject;
        }
    }

    private List<MetricTracker> metricTrackers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventSystem.current?.SetSelectedGameObject(backButton.gameObject);
        backButton.onClick.AddListener(() => Return());
        resetMetricsButton.onClick.AddListener(() => Metrics.Reset());
        metricTrackers = new List<MetricTracker>();
        HashSet<string> alwaysDisplaySet = new();
        HashSet<string> countedInSecondsSet = new();
        foreach (var key in metricsCountedInSeconds)
        {
            countedInSecondsSet.Add(key);
        }
        foreach (var key in metricsToAlwaysDisplayEvenIfUnset)
        {
            alwaysDisplaySet.Add(key);
            TrackMetric(key, countedInSecondsSet.Contains(key));
        }
        foreach (var key in Metrics.keys)
        {
            if (!alwaysDisplaySet.Contains(key))
            {
                TrackMetric(key, countedInSecondsSet.Contains(key));
            }
        }
    }

    private void TrackMetric(string key, bool countedInSeconds)
    {
        TMPro.TextMeshProUGUI textObject;
        if (metricTrackers.Count == 0)
        {
            textObject = firstMetricTextObject;
        }
        else
        {
            textObject = Instantiate(firstMetricTextObject.gameObject)
                .GetComponent<TMPro.TextMeshProUGUI>();
            textObject.transform.SetParent(
                firstMetricTextObject.transform.parent
            );
            var srcRectTransform = firstMetricTextObject.GetComponent<RectTransform>();
            var dstRectTransform = textObject.GetComponent<RectTransform>();
            Misc.TransferTypeProperties(typeof(RectTransform), srcRectTransform, dstRectTransform);
            dstRectTransform.localPosition -= Vector3.up*30.0f*metricTrackers.Count;
            dstRectTransform.ForceUpdateRectTransforms();
        }
        textObject.text = "";
        metricTrackers.Add(new MetricTracker(key, countedInSeconds, textObject));
    }

    void Update()
    {
        foreach (var tracker in metricTrackers)
        {
            if (tracker.countedInSeconds)
            {
                int raw = (int) (Metrics.Get(tracker.key));
                int seconds = raw%60;
                int minutes = (raw/60)%60;
                int hours = raw/3600;
                tracker.textObject.text =
                    $"{tracker.key}: {hours}:{minutes:D2}:{seconds:D2}";
            }
            else
            {
                tracker.textObject.text =
                    $"{tracker.key}: {(int) (Metrics.Get(tracker.key))}";
            }
        }
    }
}
