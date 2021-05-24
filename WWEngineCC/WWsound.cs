using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
namespace WWsound
{
    class WWsound
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Unicode)]
        extern static ulong MciSendString(string command, string buffer, int bufferSize, IntPtr callback);
        private List<string> mscStore;
        private List<int> state;
        //state: 0-未放入内存 1-已放入内存 2-正在播放 3-暂停中
        Dictionary<string, int> reflect;
        private int ptr;
        /// <summary>
        /// 构造函数
        /// </summary>
        public WWsound()
        {
            ptr = 0;
            mscStore = new List<string>();
            state = new List<int>();
            reflect = new Dictionary<string, int>();
        }

        /// <summary>
        /// 构造函数,会将路径文件读入内存
        /// </summary>
        /// <param name="path"></param>
        public WWsound(string path) 
        {
            ptr = 0;
            mscStore = new List<string>();
            state = new List<int>();
            reflect = new Dictionary<string, int>();
            loadMsc(path);
        }
        /// <summary>
        /// 传入路径,将路径文件读入内存,并返回编号
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int loadMsc(string path) 
        {
            MciSendString("open " + path, null, 0, IntPtr.Zero);
            mscStore.Add(path);
            state.Add(1);
            reflect[path] = ptr;
            ptr++;
            return ptr-1;
        }

        /// <summary>
        /// 传入路径播放音乐,第二个参数默认为false,传入true时将循环播放
        /// 传入路径文件未加载视为不合法输入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="repeat"></param>
        /// <returns></returns>
        public bool playMsc(string path,bool repeat=false) 
        {
            if ((!reflect.ContainsKey(path))||(state[reflect[path]]==0))
                return false;
            if(repeat)
            {
                MciSendString("play" + path + " repeat", null, 0, IntPtr.Zero);
                int indd = reflect[path];
                state[indd] = 2;
                return true;
            }
            MciSendString("play "+path, null, 0, IntPtr.Zero);
            int ind = reflect[path];
            state[ind] = 2;
            return true;
        }

        /// <summary>
        /// 传入音乐序号,
        /// </summary>
        /// <param name="cur"></param>
        /// <returns></returns>
        public bool playMsc(int cur,bool repeat=false)
        {
            if (cur >= ptr)
                return false;
            else if (repeat)
                MciSendString("play " + mscStore[cur] + " repeat", null, 0, IntPtr.Zero);
            else
                MciSendString("play "+mscStore[cur], null, 0, IntPtr.Zero);
            state[cur] = 2;
            return true;
        }
        /// <summary>
        /// 传入路径进行暂停
        /// </summary>
        /// <param name="path"></param>
        /// <param name="toAll"></param>
        /// <returns></returns>
        public bool pauseMsc(string path)
        {
            if (!reflect.ContainsKey(path))
                return false;
            int indd = reflect[path];
            if (state[indd] != 2)
                return false;
            MciSendString("pause "+path, null, 0, IntPtr.Zero);
            state[indd] = 3;
            return true;
        }
        /// <summary>
        /// 传入序号暂停对应音乐
        /// </summary>
        /// <param name="cur"></param>
        /// <returns></returns>
        public bool pauseMsc(int cur)
        {
            if ((cur >= ptr)||(state[cur]!=2))
                return false;
            MciSendString("pause "+mscStore[cur], null, 0, IntPtr.Zero);
            state[cur] = 3;
            return true;
        }
        /// <summary>
        /// 暂停所有正在播放的音乐
        /// </summary>
        public void pauseAll()
        {
            for (int i = 0; i < ptr; i++)
                pauseMsc(i);
        }

        /// <summary>
        /// 传入路径,继续播放该音乐
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool continuePlay(string path)
        {
            if (!reflect.ContainsKey(path))
                return false;
            int ind = reflect[path];
            if (state[ind]!=3)
                return false;
            MciSendString("resume " + path, null, 0, IntPtr.Zero);
            state[ind] = 2;
            return true;
        }

        /// <summary>
        /// 传入序号,继续播放该音乐
        /// </summary>
        /// <param name="cur"></param>
        /// <returns></returns>
        public bool continuePlay(int cur)
        {
            if ((cur >= ptr)||(state[cur]!=3))
                return false;
            MciSendString("resume " + mscStore[cur], null, 0, IntPtr.Zero);
            state[cur] = 2;
            return true;
        }
        /// <summary>
        /// 继续播放所有暂停中音乐
        /// </summary>
        public void continueAll()
        {
            for(int i=0;i<ptr;i++)
            {
                if (state[i] == 3)
                    continuePlay(i);
            }
        }
        /// <summary>
        /// 传入路径将路径文件从内存去除
        /// 传入all则移除所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool deLoad(string path)
        {
            if ((!reflect.ContainsKey(path)) || (state[reflect[path]] == 0))
                return false;
            MciSendString("close " + path, null, 0, IntPtr.Zero);
            state[reflect[path]] = 0;
            return true;
        }

        /// <summary>
        /// 传入序号,从内存中取出序号资源
        /// </summary>
        /// <param name="cur"></param>
        /// <returns></returns>
        public bool deLoad(int cur)
        {
            if ((cur>=ptr)||(cur<0))
                return false;
            MciSendString("close " + mscStore[cur], null, 0, IntPtr.Zero);
            state[cur] = 0;
            return true;
        }

        /// <summary>
        /// 从内存中移除所有资源
        /// </summary>
        public void deLoadAll()
        {
            for (int i = 0; i < ptr; i++)
                if (state[i] != 0)
                    deLoad(i);
        }

        /// <summary>
        /// 传入list,获取已经缓存的音乐列表
        /// </summary>
        /// <param name="res"></param>
        public void getList(ref List<string> res)
        {
            for (int i = 0; i < ptr; i++)
                res.Add(mscStore[i]);
        }

        /// <summary>
        /// 获取音乐列表大小
        /// </summary>
        /// <returns></returns>
        public int size()
        {
            return ptr;
        }

        /// <summary>
        /// 设置音量在0-1000
        /// 传入范围外的值会进行转化
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool setVolume(int level)
        {
            if (level > 1000)
                level = 1000;
            if (level < 0)
                level = 0;
            string MciCommand = string.Format("setaudio NOWMUSIC volume to {0}", level);
            MciSendString(MciCommand , null, 0, IntPtr.Zero);
            return true;
        }
        /// <summary>
        /// 通配符匹配,用于匹配路径
        /// </summary>
        /// <param name="s"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool match(string s,string p)
        {
            int i = 0;
            int j = 0;
            int startPos = -1;
            int match = -1;
            while (i < s.Length)
            {
                if (j < p.Length && (s[i] == p[j] || p[j] == '?'))
                {
                    i++;
                    j++;
                }
                else if (j < p.Length && p[j] == '*')
                {
                    startPos = j;
                    match = i;
                    j = startPos + 1;
                }
                else if (startPos != -1)
                {
                    match++;
                    i = match;
                    j = startPos + 1;
                }
                else
                {
                    return false;
                }
            }
            while (j < p.Length && p[j] == '*') j++;
            return j == p.Length;
        }
    }
}
