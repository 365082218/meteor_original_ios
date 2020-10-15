using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//不能ping域名，没什么卵用，还要自己dns转换一下
public class PingTool : MonoBehaviour {
    // 最大尝试ping的次数
    private static int nMaxTryNum = 10;
    // 检测频率
    private static float nCheckInterval = 3f;
    // 需要 ping 的 IP
    private static string strRemoteIP = "";
    private static PingTool pingTool;
    public static void CreatePing(string strIP) {
        if (string.IsNullOrEmpty(strIP)) return;

        if (pingTool != null) {
            Debug.Log("Please Stop Ping Before.");
            return;
        }

        strRemoteIP = strIP;

        // 复用组件，避免频繁的创建和销毁组件
        GameObject go = GameObject.Find("PingTool");
        if (go == null) {
            go = new GameObject("PingTool");
            DontDestroyOnLoad(go);
        }

        pingTool = go.AddComponent<PingTool>();
    }

    public static void StopPing() {
        if (pingTool != null) {
            pingTool.CancelInvoke();
            Destroy(pingTool);
        }
    }

    public static void SetCheckInterval(float value) {
        nCheckInterval = value;
    }


    private void Start() {
        InvokeRepeating("Execute", 0, nCheckInterval);
    }

    private void Execute() {
        if (pingTool == null) return;

        StartCoroutine(PingConnect());
    }

    private void Destroy() {
        strRemoteIP = "";
        nCheckInterval = 1.0f;

        pingTool = null;
    }

    private IEnumerator PingConnect() {
        if (pingTool != null) {
            Ping ping = new Ping(strRemoteIP);

            int nTryNum = 0;
            while (!ping.isDone) {
                yield return new WaitForSeconds(2.0f);

                // Ping Fail
                if (nTryNum++ > nMaxTryNum) {
                    yield break;
                }
            }

            if (ping.isDone) {
                int nDelayTime = ping.time;
                Debug.Log("nDelayTime : " + nDelayTime.ToString() + "\t" + Time.time);
            } else {
                // 延时超过 3次重试

            }
        }
    }
}