using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameMain : MonoBehaviour
{
    [SerializeField] private AudioSource candy_audio;
    [SerializeField] private TextMeshProUGUI textLV;
    [SerializeField] private GameObject successMenu;
    [SerializeField] private int candyTubeCount;// so tube chua candy ban dau
    [SerializeField] private Transform[] tubes;
    [SerializeField] private Sprite[] candySprites;
    [SerializeField] private GameObject candyPrefab;
    private GameObject selectedCandy = null;
    private int maxCandy = 4;

    // Start is called before the first frame update
    void Start()
    {
        textLV.text = "LV." + LoadLV();
        CreateCandy();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
            PlayerPrefs.DeleteAll();
    }

    private void CreateCandy()
    {
        candyTubeCount = 3 + Mathf.Min((LoadLV() - 1) / 5, 4);// sau 5lv thi tang 1 tube

        foreach (var tube in tubes)
            tube.gameObject.SetActive(false);
        for (int i = 0; i < candyTubeCount + 2; i++)
            tubes[i].gameObject.SetActive(true);

        List<int> spriteIndices = new List<int>();// ds chi so sprite
        int spriteCount = candyTubeCount;
        for (int i = 0; i < candyTubeCount * maxCandy; i++)
            spriteIndices.Add(i % spriteCount);// chia deu cac sprite

        spriteIndices = spriteIndices.OrderBy(s => Random.value).ToList();

        //gan candy
        int index = 0;
        foreach (var tube in tubes.Take(candyTubeCount))
        {
            for (int i = 0; i < maxCandy; i++)
            {
                GameObject candy = Instantiate(candyPrefab, tube);
                candy.transform.localPosition = new Vector3(0, -150 + 90 * i, 0);

                Sprite randomSprite = candySprites[spriteIndices[index]];
                candy.GetComponent<Image>().sprite = randomSprite;
                index++;
            }
        }
    }

    public void OnTubeClicked(Transform tube)
    {
        if (selectedCandy == null)
        {
            if (tube.childCount > 0)
            {
                Transform topCandy = tube.GetChild(tube.childCount - 1);
                selectedCandy = topCandy.gameObject;

                Vector3 targetLocalPos = new Vector3(0, -150 + 90 * maxCandy, 0);// pos candy move len tren tube
                Vector3 targetWorldPos = tube.TransformPoint(targetLocalPos);// c/doi targetLocalPos từ k jan cục bộ -> k gian TG
                StartCoroutine(MoveCandy(selectedCandy, targetWorldPos));
            }
        }
        else
        {
            if (tube.childCount <= maxCandy)
            {
                bool newTube = selectedCandy.transform.parent != tube;
                if (newTube && tube.childCount == maxCandy) return;

                int candyIndex = newTube ? tube.childCount : tube.childCount - 1;

                Vector3 newTubePos = tube.TransformPoint(new Vector3(0, -150 + 90 * maxCandy, 0));
                Vector3 midPoints = (selectedCandy.transform.position + newTubePos) / 2 + Vector3.up * 90f;
                Vector3 targetWorldPos = tube.TransformPoint(new Vector3(0, -150 + 90 * candyIndex, 0));

                StartCoroutine(newTube ? MoveCandyWithCurve(selectedCandy, midPoints, newTubePos, targetWorldPos) :
                    MoveCandy(selectedCandy, targetWorldPos));

                selectedCandy.transform.SetParent(tube);// gan lai candy vao tube
                selectedCandy = null;

                CheckWinCondition();
            }
        }
    }

    private IEnumerator MoveCandy(GameObject candy, Vector3 targetPos)
    {
        candy_audio.Play();
        float elapsed = 0, duration = 0.1f;

        while (elapsed < duration)
        {
            candy.transform.position = Vector3.Lerp(candy.transform.position, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        candy.transform.position = targetPos;
    }

    private IEnumerator MoveCandyWithCurve(GameObject candy, Vector3 midPoint, Vector3 newTubePos, Vector3 targetPos)
    {
        float elapsed = 0, duration = 0.2f;

        Vector3 startPos = candy.transform.position;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            candy.transform.position = Vector3.Lerp(Vector3.Lerp(startPos, midPoint, t),
                Vector3.Lerp(midPoint, newTubePos, t), t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        candy.transform.position = newTubePos;

        yield return new WaitForSeconds(0.05f);
        yield return MoveCandy(candy, targetPos);
    }

    private void CheckWinCondition()
    {
        int index = 0;
        foreach (var tube in tubes)
        {
            if (tube.childCount != maxCandy) continue;

            Sprite firstSprite = tube.GetChild(0).GetComponent<Image>().sprite;
            bool isValidTube = true;
            for (int i = 1; i < maxCandy; i++)
            {
                Sprite currentSprite = tube.GetChild(i).GetComponent<Image>().sprite;
                if (currentSprite != firstSprite)
                {
                    isValidTube = false;
                    break;
                }
            }

            if (isValidTube) index++;
        }

        if (index == candyTubeCount)
            Invoke("Success", 0.5f);
    }

    private void Success() => successMenu.SetActive(true);

    public void UpdateLV()
    {
        PlayerPrefs.SetInt("level", LoadLV() + 1);
        textLV.text = "LV." + LoadLV();
    }

    private int LoadLV() => PlayerPrefs.GetInt("level", 1);
}
