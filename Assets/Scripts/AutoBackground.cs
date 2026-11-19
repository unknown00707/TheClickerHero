using UnityEngine;

public class AutoBackground : MonoBehaviour
{
    [Header("Data")]
    public int Id; // 검사용
    public SpriteRenderer spriteRenderer;
    public AutoBackground secondSelf;
    private Vector3 startPos;
    private Vector3 endPos;
    private float moveSpeed; // 이동 속도

    private bool isSetReady;
    private bool isSecond;

    void Awake()
    {
        isSecond = secondSelf == this;
    }
    // Update is called once per frame  
    void Update()
    {
        if(transform.position.x > endPos.x)
        {
            transform.position = startPos;
            
        }
        if(isSetReady)
            transform.position += moveSpeed * Time.deltaTime * Vector3.right;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        if(!isSecond)
            secondSelf.SetMoveSpeed(speed);
    }

    public void SetNewSprite(Sprite newSptire)
    {
        spriteRenderer.sprite = newSptire;
        if(!isSecond)
            secondSelf.SetNewSprite(newSptire);
    }

    public void InitPos(Vector3 start, Vector3 end, float offsetx, bool isInit = false)
    {
        startPos = start;
        endPos = end;
        if(!isSecond)
            secondSelf.InitPos(start, end, offsetx, isInit);

        if (isInit)
        {
            transform.position = new (start.x + offsetx, start.y, start.z);
            if(isSecond)
                transform.position = start;
        }
    }

    public void SetReady(bool ready)
    {
        isSetReady = ready;
        if(!isSecond)
            secondSelf.SetReady(ready);
    }
}
