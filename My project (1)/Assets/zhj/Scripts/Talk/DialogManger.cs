using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using TMPro;

using UnityEngine.UI;



public class DiaLogmanager : MonoBehaviour

{

  /// <summary>

  /// 对话内容文本，csv格式

  /// </summary> 

  public TextAsset dialogDataFile;



  /// <summary>

  /// 左侧角色图像

  /// </summary>

  public SpriteRenderer spriteLeft;

  /// <summary>

  /// 右侧角色图像

  /// </summary>

  public SpriteRenderer spriteRight;



  /// <summary>

  /// 角色名字文本

  /// </summary>

  public TMP_Text nameText;



  /// <summary>

  /// 对话内容文本

  /// </summary>

  public TMP_Text dialogText;



  /// <summary>

  /// 角色图片列表

  /// </summary>

  public List<Sprite> sprites = new List<Sprite>();



  /// <summary>

  /// 角色名字对应图片的字典

  /// </summary>

  Dictionary<string, Sprite> imageDic = new Dictionary<string, Sprite>();

  /// <summary>

  /// 当前对话索引值

  /// </summary>

  public int dialogIndex;

  /// <summary>

  /// 对话文本按行分割

  /// </summary>

  public string[] dialogRows;

  /// <summary>

  /// 继续按钮

  /// </summary>

  public Button next;



  /// <summary>

  /// 选项按钮

  /// </summary>

  public GameObject optionButton;

  /// <summary>

  /// 选项按钮父节点

  /// </summary>

  public Transform buttonGroup;

  // Start is called before the first frame update

  private void Awake()

  {

    imageDic["人"] = sprites[0];

    imageDic["人鱼"] = sprites[1];

  }

  void Start()

  {

    ReadText(dialogDataFile);

    ShowDiaLogRow();

    // UpdateText("安吉丽娜", "即使引导早已破碎,也请您当上艾尔登之王");

    //UpdateImage("僵尸", false);//不在左侧

    // UpdateImage("安吉丽娜", true);//在左侧

  }

  // Update is called once per frame

  void Update()

  {



  }





  //更新文本信息

  public void UpdateText(string _name, string _text)

  {

    nameText.text = _name;

    dialogText.text = _text;

  }

  //更新图片信息

  public void UpdateImage(string _name, string _position)

  {

    // 1. 声明接收字典值的变量（根据你的 imageDic 值类型调整，此处假设为 Sprite）
    Sprite targetSprite = null;
    
    // 2. 尝试从字典中取值，返回 bool 表示是否取值成功
    if (imageDic.TryGetValue(_name, out targetSprite))
    {
        // 3. 取值成功（键存在），执行图片赋值逻辑
        if (_position == "左")
        {
            spriteLeft.sprite = targetSprite;
        }
        else if (_position == "右")
        {
            spriteRight.sprite = targetSprite;
        }
    }
    else
    {
        // 4. 取值失败（键不存在），兜底处理（避免崩溃，便于排查问题）
        Debug.LogError($"字典 imageDic 中不存在键：{_name}，无法更新图片");
        // 可选：赋值默认图片，保证界面显示正常
        // Sprite defaultSprite = Resources.Load<Sprite>("Default/DefaultCharacter");
        // if (_position == "左") spriteLeft.sprite = defaultSprite;
        // else if (_position == "右") spriteRight.sprite = defaultSprite;
    }

  }



  public void ReadText(TextAsset _textAsset)

  {

    dialogRows = _textAsset.text.Split('\n');//以换行来分割

                         // foreach(var row in rows)

                         //{

                         // string[] cell = row.Split(',');

                         // }

    Debug.Log("读取成果");

  }



  public void ShowDiaLogRow()

  {

    for(int i=0;i<dialogRows.Length;i++)

    {

      string[] cells = dialogRows[i].Split(',');

      if (cells[0] == "#" && int.Parse(cells[1]) == dialogIndex)

      {

        UpdateText(cells[2], cells[4]);

        UpdateImage(cells[2], cells[3]);



        dialogIndex = int.Parse(cells[5]);

        next.gameObject.SetActive(true);

        break;

      }

      else if (cells[0]== "&" && int.Parse(cells[1]) == dialogIndex)

      {

        next.gameObject.SetActive(false);//隐藏原来的按钮

        GenerateOption(i);

      }

      else if (cells[0] == "end" && int.Parse(cells[i]) == dialogIndex)

      {

        Debug.Log("剧情结束");//这里结束

      }

    }

  }

  public void OnClickNext()

  {

    ShowDiaLogRow();

  }

  public void GenerateOption(int _index)//生成按钮

  {

    string[] cells = dialogRows[_index].Split(',');

    if (cells[0] == "&")

    {

      GameObject button = Instantiate(optionButton, buttonGroup);

      //绑定按钮事件

      button.GetComponentInChildren<TMP_Text>().text = cells[4];

      button.GetComponent<Button>().onClick.AddListener(delegate 

      { 

        OnOptionClick(int.Parse(cells[5]));

      }

      );

      GenerateOption(_index + 1);

    }

    

  }



  public void OnOptionClick(int _id)

  {

    dialogIndex = _id;

    ShowDiaLogRow();

    for(int i=0;i < buttonGroup.childCount; i++)

    {

      Destroy(buttonGroup.GetChild(i).gameObject);

    }

  }

}