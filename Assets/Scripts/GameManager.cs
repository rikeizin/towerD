using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;

    public Image[] UIHealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartButton;

    private void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }
    public void NextStage()
    {
        //Change Stage
        if (stageIndex < Stages.Length-1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE" + (stageIndex + 1);
        }
        else
        {   //Game Clear
            //Player Control Lock
            Time.timeScale = 0;
            //Restart Button UI
            Text btnText = UIRestartButton.GetComponentInChildren<Text>(); //버튼 텍스트는 자식오브젝트이므로 GetComponentInChildren을 사용해주어야 한다.
            btnText.text = "Clear!";
            ViewBtn();
        }


        //Calculate Point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIHealth[health].color = new Color(1, 0, 0, 0.4f);
         }
        else
        {
            //All Health UI Off
            UIHealth[0].color = new Color(1, 0, 0, 0.4f);

            //Player Die Effect
            player.OnDie();
            //Retry Button UI
            UIRestartButton.SetActive(true);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Player Reposition
            if (health > 1)
            {
                PlayerReposition();
            }

            //Health Down
            HealthDown();
        }
    }
    void PlayerReposition()
    {
        player.transform.position = new Vector3(-11, 2.5f, 0);
        player.VelocityZero();
    }

    void ViewBtn()
    {
        UIRestartButton.SetActive(true);
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0); // using UnityEngine.SceneManagement;를 선언해주어야 사용할 수 있다.
    }
}
