using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM3Digest
{
    public class SM3Digest
    {
        public static byte[] SM3(byte[] content)
        {
            SM3Digest sd = new SM3Digest();
            return sd.Digest(content);
        }
        SM3Engine engine;
        public SM3Digest()
        {
            this.engine = new SM3Engine(this);
            ReSet();
        }
        /// <summary>
        /// 重置SM3Digest
        /// </summary>
        public void ReSet()
        {
            for (int i = 0; i < 8; i++)
            {
                V[i] = IV[i];
            }
            engine.ReSet();
        }

        readonly static UInt32[] IV = new UInt32[] {
 0x7380166f, 0x4914b2b9, 0x172442d7, 0xda8a0600,
 0xa96f30bc, 0x163138aa, 0xe38dee4d, 0xb0fb0e4e };

        readonly static UInt32[] T = new UInt32[] {
 0x79cc4519, 0x79cc4519, 0x79cc4519, 0x79cc4519,
 0x79cc4519, 0x79cc4519, 0x79cc4519, 0x79cc4519,
 0x79cc4519, 0x79cc4519, 0x79cc4519, 0x79cc4519,
 0x79cc4519, 0x79cc4519, 0x79cc4519, 0x79cc4519,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a,
 0x7a879d8a, 0x7a879d8a, 0x7a879d8a, 0x7a879d8a };

        UInt32 FF0_15(UInt32 X, UInt32 Y, UInt32 Z)
        {
            return X ^ Y ^ Z;
        }
        UInt32 FF16_63(UInt32 X, UInt32 Y, UInt32 Z)
        {
            return (X & Y) | (X & Z) | (Y & Z);
        }
        UInt32 GG0_15(UInt32 X, UInt32 Y, UInt32 Z)
        {
            return X ^ Y ^ Z;
        }
        UInt32 GG16_63(UInt32 X, UInt32 Y, UInt32 Z)
        {
            return (X & Y) | (~X & Z);
        }
        UInt32 P0(UInt32 X)
        {
            return X ^ ((X << 9) | (X >> 23)) ^ ((X << 17) | (X >> 15));
        }
        UInt32 P1(UInt32 X)
        {
            return X ^ ((X << 15) | (X >> 17)) ^ ((X << 23) | (X >> 9));
        }

        UInt32 A, B, C, D, E, F, G, H;
        UInt32 SS1, SS2, TT1, TT2;
        UInt32[] W = new UInt32[68];
        UInt32[] W_ = new UInt32[64];
        internal UInt32[] V = new UInt32[8];


        internal void setWs(byte[] bs, int offset)
        {
            for (int i = 0; i < 16; i++)
            {
                W[i] = (UInt32)((bs[offset] << 24) + (bs[offset + 1] << 16) | (bs[offset + 2] << 8) | bs[offset + 3]);
                offset += 4;
            }
        }
        /// <summary>
        /// 每次调用前,执行 setWs 方法
        /// </summary>
        internal void DigestBlock()
        {

            for (int j = 16; j < 68; j++)
            {
                W[j] = P1(W[j - 16] ^ W[j - 9] ^ ((W[j - 3] << 15) | (W[j - 3] >> 17)))//
                ^ ((W[j - 13] << 7) | (W[j - 13] >> 25))//
                ^ W[j - 6];
            }
            for (int j = 0; j < 64; j++)
            {
                W_[j] = W[j] ^ W[j + 4];
            }

            A = V[0];
            B = V[1];
            C = V[2];
            D = V[3];
            E = V[4];
            F = V[5];
            G = V[6];
            H = V[7];

            UInt32 temp;
            for (int j = 0; j < 16; j++)
            {
                temp = ((A << 12) | (A >> 20)) + E + ((T[j] << j) | (T[j] >> (32 - j)));
                SS1 = (temp << 7) | (temp >> 25);
                SS2 = SS1 ^ ((A << 12) | (A >> 20));
                TT1 = FF0_15(A, B, C) + D + SS2 + W_[j];
                TT2 = GG0_15(E, F, G) + H + SS1 + W[j];
                D = C;
                C = (B << 9) | (B >> 23);
                B = A;
                A = TT1;
                H = G;
                G = (F << 19) | (F >> 13);
                F = E;
                E = P0(TT2);
            }
            for (int j = 16; j < 64; j++)
            {
                temp = ((A << 12) | (A >> 20)) + E + ((T[j] << j) | (T[j] >> (32 - j)));
                SS1 = (temp << 7) | (temp >> 25);
                SS2 = SS1 ^ ((A << 12) | (A >> 20));
                TT1 = FF16_63(A, B, C) + D + SS2 + W_[j];
                TT2 = GG16_63(E, F, G) + H + SS1 + W[j];
                D = C;
                C = (B << 9) | (B >> 23);
                B = A;
                A = TT1;
                H = G;
                G = (F << 19) | (F >> 13);
                F = E;
                E = P0(TT2);
            }
            V[0] ^= A;
            V[1] ^= B;
            V[2] ^= C;
            V[3] ^= D;
            V[4] ^= E;
            V[5] ^= F;
            V[6] ^= G;
            V[7] ^= H;
        }
        /// <summary>
        /// UpDate(bs[offset]~bs[offset+len])
        /// 为了加快运算速度,前面的UpDate,bs的有效长度Len最好保持为64的倍数.
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="offset">偏移量</param>
        /// <param name="len">消息长度</param>
        public void UpDate(byte[] bs, int offset, int len)
        {
            engine.UpDate(bs, offset, len);
        }
        public void UpDate(byte[] bs)
        {
            engine.UpDate(bs, 0, bs.Length);
        }
        /// <summary>
        /// 会自动重置 this
        /// </summary>
        /// <returns></returns>
        public byte[] Digest()
        {
            return engine.Digest();
        }
        /// <summary>
        /// 会自动重置 this
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public byte[] Digest(byte[] bs)
        {
            UpDate(bs);
            return engine.Digest();
        }
    }
}

namespace SM3Digest
{
    /// <summary>
    /// UpDate时消息数组通常是不规则的,
    /// 所以有必要使用一个合适的消息分组引擎,
    /// 该引擎只负责消息分组和消息填充工作.
    /// </summary>
    internal class SM3Engine
    {
        SM3Digest digest;
        UInt64 count = 0;
        public SM3Engine(SM3Digest digest)
        {
            this.digest = digest;
        }

        byte[] last = new byte[64];
        int lastSize = 0;

        public void UpDate(byte[] bs, int offset, int len)
        {
            if (bs == null) return;
            count += (UInt64)len;

            if (lastSize + len < 64)
            {
                // Console.WriteLine("update:{0},{1},{2},{3}", offset, lastSize, len,count);
                Array.Copy(bs, offset, last, lastSize, len);
                lastSize += len;
                return;
            }
            if (lastSize > 0)
            {
                Array.Copy(bs, offset, last, lastSize, 64 - lastSize);
                digest.setWs(last, 0);
                digest.DigestBlock();

                offset = offset + 64 - lastSize;
                len = len - (64 - lastSize);
            }
            while (len >= 64)
            {
                digest.setWs(bs, offset);
                digest.DigestBlock();
                offset += 64;
                len -= 64;
            }
            if (len > 0)
            {
                Array.Copy(bs, offset, last, 0, len);
            }
            lastSize = len;
        }

        public byte[] Digest()
        {
            UInt32 r = (UInt32)(count % 64);
            UInt64 countBit = this.count * 8;
            byte[] bs;
            if (r <= 55)
            {
                //Console.WriteLine("countBit:"+ countBit);
                bs = new byte[64 - r];
                bs[0] = 128;
                bs[64 - r - 8] = (byte)((countBit >> 56) & 0xff);
                bs[64 - r - 7] = (byte)((countBit >> 48) & 0xff);
                bs[64 - r - 6] = (byte)((countBit >> 40) & 0xff);
                bs[64 - r - 5] = (byte)((countBit >> 32) & 0xff);
                bs[64 - r - 4] = (byte)((countBit >> 24) & 0xff);
                bs[64 - r - 3] = (byte)((countBit >> 16) & 0xff);
                bs[64 - r - 2] = (byte)((countBit >> 8) & 0xff);
                bs[64 - r - 1] = (byte)(countBit & 0xff);
            }
            else
            {
                bs = new byte[128 - r];
                bs[0] = 128;
                bs[128 - r - 8] = (byte)((countBit >> 56) & 0xff);
                bs[128 - r - 7] = (byte)((countBit >> 48) & 0xff);
                bs[128 - r - 6] = (byte)((countBit >> 40) & 0xff);
                bs[128 - r - 5] = (byte)((countBit >> 32) & 0xff);
                bs[128 - r - 4] = (byte)((countBit >> 24) & 0xff);
                bs[128 - r - 3] = (byte)((countBit >> 16) & 0xff);
                bs[128 - r - 2] = (byte)((countBit >> 8) & 0xff);
                bs[128 - r - 1] = (byte)(countBit & 0xff);
            }

            UpDate(bs, 0, bs.Length);
            byte[] ds = new byte[32];
            for (int i = 0, j = 0; i < 8; i++)
            {
                ds[j++] = (byte)((digest.V[i] >> 24) & 0xff);
                ds[j++] = (byte)((digest.V[i] >> 16) & 0xff);
                ds[j++] = (byte)((digest.V[i] >> 8) & 0xff);
                ds[j++] = (byte)(digest.V[i] & 0xff);
            }

            digest.ReSet();//
            return ds;
        }
        /// <summary>
        /// 重置分组引擎
        /// </summary>
        public void ReSet()
        {
            count = 0;
            lastSize = 0;
        }
    }
}