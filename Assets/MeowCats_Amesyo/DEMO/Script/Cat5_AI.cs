using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MeowCats_amesyo
{
public class Cat5_AI : MonoBehaviour {


	[Header("Base Object")]
	public GameObject P_Cat;
	public GameObject MoveTarget;
	public GameObject MoveArea;												//この箱を大きくすると、動く範囲が　拡大するよ【＾＾】／

	[Header("Move Area")]
	public float L_xlimit	= 1.5f;
	public float R_xlimit	= -1.5f;
	public float F_zlimit	= -2f;
	public float B_zlimit	= 2f;
	[Header("Vit=50 Peppy  Vit=2000 Meek")]
	public float Vitality = 50;												//この数値が多いと、あまり移動しない　５０＝元気いっぱい　６００＝寝てる感じ
	public Vector3 Move_Pos = Vector3.zero;
	private CharacterController controller;
	private Animator animator;
	private Vector3 cat_init_pos;

	[Header("for animeter Status")]
	public int c_state;														//アニメーターに渡す、モーションパターン
	[Header("Flag")]
	public bool OnMove_Flag = false;
	public bool MoveTarget_MoveFlag = false;
	public bool OnAttack_Flag = false;
	[Header("重力")]
	public float gravity = 20f;
	private Vector3 moveDirection;
	public float speed = 0;
	public float step =0;
	private Vector3 CatPos;
	[Header("移動先の座標")]
	public Vector3 endPosition = Vector3.zero;

	[Header("敵の座標")]
	public Vector3 EZAHYOU;

	// Use this for initialization
	void Start () {

		L_xlimit = MoveArea.transform.localScale.x / 2;
		R_xlimit = -1 * (MoveArea.transform.localScale.x / 2);
		F_zlimit = -1 * (MoveArea.transform.localScale.z / 2);
		B_zlimit = MoveArea.transform.localScale.z / 2;

		CatPos.x = MoveArea.transform.position.x;
		CatPos.z = MoveArea.transform.position.z;
		P_Cat.transform.position = CatPos;

		RandPosition ();//範囲内でランダムにｘ，ｚを決める
	
		endPosition = Move_Pos;
		MoveTarget.transform.position = endPosition;

		controller = P_Cat.GetComponentInChildren<CharacterController>();			//キャラクターコントローラーのアサイン
		animator = P_Cat.GetComponentInChildren<Animator>();						//アニメーターのアサイン
	}
	
	// Update is called once per frame
	void Update () {

		//接地してなきゃ、接地させる
		moveDirection.y -= gravity * Time.deltaTime;
		// 移動
		controller.Move (moveDirection * Time.deltaTime);

		if (controller.isGrounded) {											//もしコントローラーが接地してたら
			MoveTarget.transform.position = endPosition;						
			// Neko Anime
			c_state = Random.Range (0, 10);
			animator.SetInteger ("pat", c_state);								// for Animation  モーションが一段落した時の次のモーションはランダムに

			//--------------------------
			if (OnMove_Flag == false) {						//移動中じゃないなら 
				animator.SetBool ("walk", false);
				animator.SetBool ("slow", false);
				animator.SetBool ("run", false);

				float cRnd = Random.Range (0, (Vitality * 2));						//３００　＝＝　300 ループに１回 (50 〜　600程度)

					if (cRnd < 1 && MoveTarget_MoveFlag == false) {	
							
						OnMove_Flag = true;
						RandPosition ();//移動場所をきめる
						//スピードとパターン決定
						SpeedPattern();
						//エンドPOS　＝　目標位置
						endPosition = Move_Pos;
						//見える化
						MoveTarget.transform.position = endPosition;

						// 距離 判定		
						if (Vector3.Distance (this.transform.position, endPosition) > 0.8) {		// 距離が 0.8 以上なら移動開始
							OnMove_Flag = true;	//移動中グラグを立てる
							MoveTarget_MoveFlag = true;
							//スピードとパターン決定
							SpeedPattern();
							}
						if (Vector3.Distance (P_Cat.transform.position, endPosition) < 0.4) {		// 目標に近かったら動かない
							OnMove_Flag = false;
							MoveTarget_MoveFlag =false;
							animator.SetBool ("slow", false);
							animator.SetBool ("walk", false);
							animator.SetBool ("run", false);
							}
					}
						 
				}
			//--------------------------
			if (OnMove_Flag) {									//移動フラグ立ってたら 
				
				float cRnd = Random.Range (0, (Vitality * 2));						//３００　＝＝　300 ループに１回 (50 〜　600程度
				if (cRnd < 1 && MoveTarget_MoveFlag == false) {

							RandPosition();
							//エンドPOS　＝　目標位置
							endPosition = Move_Pos;
							//見える化
							MoveTarget.transform.position = endPosition;
							//スピードとパターン決定
							SpeedPattern();
						}

					///////////////////////////////////
					/// 実際の移動
					///////////////////////////////////
					step = speed * Time.deltaTime;
					//移動
					P_Cat.transform.position = Vector3.MoveTowards (P_Cat.transform.position, endPosition, step);	
					// 回転　（定速）
					var relativePos = endPosition - P_Cat.transform.position;
					var rotation = Quaternion.LookRotation (relativePos);
					P_Cat.transform.rotation = rotation;
				}
				//--------------------------
				if (Vector3.Distance (P_Cat.transform.position, endPosition) < 0.4) {		// 目標に近づいたら、停止
				MoveTarget_MoveFlag =false;
				OnMove_Flag = false;
				animator.SetBool ("slow", false);
				animator.SetBool ("walk", false);
				animator.SetBool ("run", false);
					
				}
			}
		} 

	//ランダムにポジション決定
	void RandPosition(){
		Move_Pos.x = Random.Range (MoveArea.transform.position.x + R_xlimit, MoveArea.transform.position.x + L_xlimit);
		Move_Pos.z = Random.Range (MoveArea.transform.position.z + F_zlimit, MoveArea.transform.position.z + B_zlimit);
		Move_Pos.y = P_Cat.transform.localPosition.y;
	}

	//ランダムにスピード決定し、パターンもそれに合わせる
	void SpeedPattern(){
		
		animator.SetBool ("slow", false);
		animator.SetBool ("walk", false);
		animator.SetBool ("run", false);

		float sRnd = Random.Range (0, 5);

		if (sRnd >= 0 && sRnd < 2) {			//2未満　＝　走る
			speed = 2;
			animator.SetBool ("run", true);
		} 
		if (sRnd >= 2 && sRnd < 4.5f) {
			speed = 1;
			animator.SetBool ("walk", true);
		}
		if (sRnd >= 4.5F ) {
			speed = 0.3F;
			//animator.SetBool ("slow", true);
			animator.SetBool ("walk", true);
		}

	}
	
}
}
