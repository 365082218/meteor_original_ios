using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
namespace Idevgame.Util
{
    public class MathUtility
    {
        public static void Rotate(ref float x, ref float z, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);
            float locationX = sin * z + cos * x;
            float locationZ = cos * z - sin * x;
            x = locationX;
            z = locationZ;
        }

        public static float DistanceSqr(Vector3 p0, Vector3 p1)
        {
            return Vector3.SqrMagnitude(p0 - p1);
        }

        public static float DistanceMax(float x1, float y1, float x2, float y2)
        {
            return Mathf.Max(Mathf.Abs(x1 - x2), Mathf.Abs(y1 - y2));
        }

        public static float DistanceMax(Vector3 p0, Vector3 p1)
        {
            return Mathf.Max(Mathf.Abs(p0.x - p1.x), Mathf.Abs(p0.z - p1.z));
        }

        public static float CrossProduct(float x0, float y0, float x1, float y1) { return x0 * y1 - y0 * x1; }

        //---------------------------------------------------------------------------
        public static bool RectangleHitDefineCollision(
            Vector3 HitDefPos, float HitDefOrientation,
            Vector3 HitDef,
            Vector3 AttackeePos, float AttackeeOrientation,
            Vector3 AttackeeBounding)
        {
            //排除高度影响，以XZ平面坐标作为判定基准
            if (HitDefPos.y > AttackeePos.y + AttackeeBounding.y ||
                AttackeePos.y > HitDefPos.y + HitDef.y)
            {
                return false;
            }

            // 计算出第一个四边形的四个定点
            float x0 = -HitDef.x * 0.5f, z0 = -HitDef.z * 0.5f;
            float x1 = -HitDef.x * 0.5f, z1 = HitDef.z * 0.5f;
            Rotate(ref x0, ref z0, HitDefOrientation);
            Rotate(ref x1, ref z1, HitDefOrientation);
            Vector2 maxHit = new Vector2(Mathf.Max(Mathf.Abs(x0), Mathf.Abs(x1)), Mathf.Max(Mathf.Abs(z0), Mathf.Abs(z1)));
            float[] HitDefPointX = new float[4] {
            HitDefPos.x - x0,
            HitDefPos.x - x1,
            HitDefPos.x + x0,
            HitDefPos.x + x1};
            float[] HitDefPointZ = new float[4] {
            HitDefPos.z - z0,
            HitDefPos.z - z1,
            HitDefPos.z + z0,
            HitDefPos.z + z1};

            // 计算出第二个四边形的四个顶点
            x0 = -AttackeeBounding.x * 0.5f;
            z0 = -AttackeeBounding.z * 0.5f;
            x1 = -AttackeeBounding.x * 0.5f;
            z1 = AttackeeBounding.z * 0.5f;
            Rotate(ref x0, ref z0, AttackeeOrientation);
            Rotate(ref x1, ref z1, AttackeeOrientation);
            Vector2 maxAtk = new Vector2(Mathf.Max(Mathf.Abs(x0), Mathf.Abs(x1)), Mathf.Max(Mathf.Abs(z0), Mathf.Abs(z1)));
            float[] AttackeePointX = new float[4] {
            AttackeePos.x - x0,
            AttackeePos.x - x1,
            AttackeePos.x + x0,
            AttackeePos.x + x1};
            float[] AttackeePointZ = new float[4] {
            AttackeePos.z - z0,
            AttackeePos.z - z1,
            AttackeePos.z + z0,
            AttackeePos.z + z1};

            if (HitDefPos.x > AttackeePos.x + maxHit[0] + maxAtk[0] ||
                HitDefPos.x < AttackeePos.x - maxHit[0] - maxAtk[0] ||
                HitDefPos.z > AttackeePos.z + maxHit[1] + maxAtk[1] ||
                HitDefPos.z < AttackeePos.z - maxHit[1] - maxAtk[1])
                return false;

            // 拿四边形的四个顶点判断，是否在另外一个四边形的四条边的一侧
            for (int i = 0; i < 4; i++)
            {
                x0 = HitDefPointX[i];
                x1 = HitDefPointX[(i + 1) % 4];
                z0 = HitDefPointZ[i];
                z1 = HitDefPointZ[(i + 1) % 4];

                bool hasSameSidePoint = false;
                for (int j = 0; j < 4; j++)
                {
                    float v = CrossProduct(x1 - x0, z1 - z0, AttackeePointX[j] - x0, AttackeePointZ[j] - z0);
                    if (v < 0)
                    {
                        hasSameSidePoint = true;
                        break;
                    }
                }

                // 如果4个定点都在其中一条边的另外一侧，说明没有交点
                if (!hasSameSidePoint)
                    return false;
            }

            // 所有边可以分割另外一个四边形，说明有焦点。
            return true;
        }

        //---------------------------------------------------------------------------
        public static bool CylinderHitDefineCollision(
            Vector3 HitDefPos, float HitDefOrientation,
            float HitRadius, float HitDefHeight,
            Vector3 AttackeePos, float AttackeeOrientation,
            Vector3 AttackeeBounding)
        {
            //排除高度影响，以XZ平面坐标作为判定基准
            if (HitDefPos.y > AttackeePos.y + AttackeeBounding.y ||
                AttackeePos.y > HitDefPos.y + HitDefHeight)
                return false;

            float vectz = HitDefPos.z - AttackeePos.z;
            float vectx = HitDefPos.x - AttackeePos.x;
            if (vectx != 0 || vectz != 0)
                Rotate(ref vectx, ref vectz, -AttackeeOrientation);

            if ((Mathf.Abs(vectx) > (HitRadius + AttackeeBounding.z)) || (Mathf.Abs(vectz) > (HitRadius + AttackeeBounding.x)))
                return false;

            return true;
        }


        //---------------------------------------------------------------------------
        public static bool RingHitDefineCollision(
            Vector3 HitDefPos, float HitDefOrientation,
            float HitInnerRadius, float HitDefHeight, float HitOutRadius,
            Vector3 AttackeePos, float AttackeeOrientation,
            Vector3 AttackeeBounding)
        {
            //排除高度影响，以XZ平面坐标作为判定基准
            if (HitDefPos.y > AttackeePos.y + AttackeeBounding.y ||
                AttackeePos.y > HitDefPos.y + HitDefHeight)
                return false;

            float radius = Mathf.Min(AttackeeBounding.x, AttackeeBounding.z);
            float distance = (AttackeePos - HitDefPos).magnitude;
            if (distance + radius < HitInnerRadius || distance - radius > HitOutRadius)
                return false;

            return true;
        }
        //---------------------------------------------------------------------------
        public static bool FanDefineCollision(
                    Vector3 HitDefPos, float HitDefOrientation,
                    float HitRadius, float HitDefHeight, float StartAngle, float EndAngle,
                    Vector3 AttackeePos, float AttackeeOrientation,
                    Vector3 AttackeeBounding)
        {
            //HitDefPos 扇形攻击框位置 
            //HitDefOrientation 扇形攻击框朝向
            //HitRadius 扇形攻击框半径
            //HitDefHeight 扇形攻击框高度
            //StartAngle 扇形攻击框开始角度
            //EndAngle 扇形攻击框结束角度
            //AttackeePos 受击框位置
            //AttackeeOrientation 受击框朝向
            //AttackeeBounding  受击框(长宽高)

            //排除高度影响，以XZ平面坐标作为判定基准
            if (HitDefPos.y > AttackeePos.y + AttackeeBounding.y ||
                AttackeePos.y > HitDefPos.y + HitDefHeight)
                return false;

            //圆心的坐标转化到被攻击者的坐标系去
            float vectz = AttackeePos.z - HitDefPos.z;
            float vectx = AttackeePos.x - HitDefPos.x;
            Rotate(ref vectz, ref vectx, HitDefOrientation);

            float hitRadius = HitRadius;

            float attackCenter_x = vectz;
            float attackCenter_y = vectx;

            float attackRadius = AttackeeBounding.x > AttackeeBounding.z ? AttackeeBounding.z : AttackeeBounding.x;

            float centerDis = Distance(0, 0, attackCenter_x, attackCenter_y);
            if (centerDis > (hitRadius + attackRadius)) //相离
                return false;

            if ((centerDis <= attackRadius))
                return true;

            float start_rad = StartAngle * Mathf.Deg2Rad;
            float end_rad = EndAngle * Mathf.Deg2Rad;


            float center_angle = 0.5f * (start_rad + end_rad);
            float axis_x = Mathf.Cos(center_angle);
            float axis_y = Mathf.Sin(center_angle);

            float len = Mathf.Sqrt(attackCenter_x * attackCenter_x + attackCenter_y * attackCenter_y);
            float temp_x = attackCenter_x / len;
            float temp_y = attackCenter_y / len;

            float dot = axis_x * temp_x + temp_y * axis_y;

            float value = Mathf.Cos(0.5f * (end_rad - start_rad));
            if (dot >= value)
                return true;

            float dis1 = PointToLineSegmentDistance(attackCenter_x, attackCenter_y, 0, 0,
                hitRadius * Mathf.Cos(start_rad), hitRadius * Mathf.Sin(start_rad));
            float dis2 = PointToLineSegmentDistance(attackCenter_x, attackCenter_y, 0, 0,
                hitRadius * Mathf.Cos(end_rad), hitRadius * Mathf.Sin(end_rad));

            if ((dis1 <= attackRadius) || (dis2 <= attackRadius))
                return true;

            return false;
        }

        static float PointToLineSegmentDistance(float x, float y, float x1, float y1, float x2, float y2)
        {
            float v1_x = x2 - x1;
            float v1_y = y2 - y1;

            //at + b = 0
            float a = (x2 - x1) * v1_x + (y2 - y1) * v1_y;
            float b = v1_x * (x1 - x) + v1_y * (y1 - y);

            if (a == 0.0f)
                return 0;

            float t = -b / a;

            //因为是线段,不是直线,处理极端的情况
            if (t < 0)
                t = 0;
            if (t > 1)
                t = 1;

            float point_x = x1 + t * (x2 - x1);
            float point_y = y1 + t * (y2 - y1);

            return Distance(point_x, point_y, x, y);
        }

        public static float Distance(float x1, float y1, float x2, float y2)
        {
            float deletax = x1 - x2;
            float deletay = y1 - y2;
            return Mathf.Sqrt(deletax * deletax + deletay * deletay);
        }

        //判断两个向量的夹角是否在某个范围之内
        public static bool IsIn2DAngle(Vector3 forward, Vector3 dir, int fromAngle, int toAngle)
        {
            forward.y = 0;
            dir.y = 0;
            int angle = (int)angle_360(forward, dir);
            fromAngle = fromAngle % 360;
            toAngle = toAngle % 360;
            if (fromAngle > toAngle)
            {
                return ((angle >= fromAngle && angle < 360) || (angle >= 0 && angle < toAngle));
            }
            else
            {
                return (angle >= fromAngle && angle < toAngle);
            }
        }
        public static float angle_360(Vector3 from_, Vector3 to_)
        {
            return angle_360(new Vector2(from_.x, from_.z), new Vector2(to_.x, to_.z));
        }
        public static float angle_360(Vector2 from_, Vector2 to_)
        {
            Vector3 v3 = Vector3.Cross(from_, to_);
            if (v3.z > 0)
                return 360 - Vector2.Angle(from_, to_);
            else
                return Vector2.Angle(from_, to_);
        }
        public static bool IsInRange(Transform checkTransform, Vector3 position, int fromAngle, int toAngle, float distance)
        {
            Quaternion r = checkTransform.rotation;
            //Vector3 f0 = (checkTransform.position + (r * Vector3.forward) * distance);

            Quaternion r0 = Quaternion.Euler(checkTransform.rotation.eulerAngles.x, checkTransform.rotation.eulerAngles.y + fromAngle, checkTransform.rotation.eulerAngles.z);
            Quaternion r1 = Quaternion.Euler(checkTransform.rotation.eulerAngles.x, checkTransform.rotation.eulerAngles.y + toAngle, checkTransform.rotation.eulerAngles.z);

            Vector3 f1 = (checkTransform.position + (r0 * Vector3.forward) * distance);
            Vector3 f2 = (checkTransform.position + (r1 * Vector3.forward) * distance);
            return isINTriangle(position, checkTransform.position, f1, f2);
        }

        public static float triangleArea(float v0x, float v0y, float v1x, float v1y, float v2x, float v2y)
        {
            return Mathf.Abs((v0x * v1y + v1x * v2y + v2x * v0y
                              - v1x * v0y - v2x * v1y - v0x * v2y) / 2f);
        }

        public static bool isINTriangle(Vector3 point, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            float x = point.x;
            float y = point.z;

            float v0x = v0.x;
            float v0y = v0.z;

            float v1x = v1.x;
            float v1y = v1.z;

            float v2x = v2.x;
            float v2y = v2.z;

            float t = triangleArea(v0x, v0y, v1x, v1y, v2x, v2y);
            float a = triangleArea(v0x, v0y, v1x, v1y, x, y) + triangleArea(v0x, v0y, x, y, v2x, v2y) + triangleArea(x, y, v1x, v1y, v2x, v2y);

            if (Mathf.Abs(t - a) <= 0.01f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}