using System;

namespace YaVipCore.Helper
{
    /// <summary>
    /// 代码来自http://www.cnblogs.com/stone_w/archive/2012/08/16/2642679.html
    /// </summary>
    public class StringCompute
    {
        #region 私有变量
        /// <summary>
        /// 字符串1
        /// </summary>
        private char[] _arrChar1;
        /// <summary>
        /// 字符串2
        /// </summary>
        private char[] _arrChar2;
        /// <summary>
        /// 统计结果
        /// </summary>
        private Result _result;
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _beginTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _endTime;
        /// <summary>
        /// 计算次数
        /// </summary>
        private int _computeTimes;
        /// <summary>
        /// 算法矩阵
        /// </summary>
        private int[,] _matrix;
        /// <summary>
        /// 矩阵列数
        /// </summary>
        private int _column;
        /// <summary>
        /// 矩阵行数
        /// </summary>
        private int _row;
        #endregion
        #region 属性
        public Result ComputeResult => _result;

        #endregion
        #region 构造函数
        public StringCompute(string str1, string str2)
        {
            StringComputeInit(str1, str2);
        }
        public StringCompute()
        {
        }
        #endregion
        #region 算法实现
        /// <summary>
        /// 初始化算法基本信息
        /// </summary>
        /// <param name="str1">字符串1</param>
        /// <param name="str2">字符串2</param>
        private void StringComputeInit(string str1, string str2)
        {
            _arrChar1 = str1.ToCharArray();
            _arrChar2 = str2.ToCharArray();
            _result = new Result();
            _computeTimes = 0;
            _row = _arrChar1.Length + 1;
            _column = _arrChar2.Length + 1;
            _matrix = new int[_row, _column];
        }
        /// <summary>
        /// 计算相似度
        /// </summary>
        public void Compute()
        {
            //开始时间
            _beginTime = DateTime.Now;
            //初始化矩阵的第一行和第一列
            InitMatrix();
            for (int i = 1; i < _row; i++)
            {
                for (int j = 1; j < _column; j++)
                {
                    var intCost = _arrChar1[i - 1] == _arrChar2[j - 1] ? 0 : 1;
                    //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                    //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                    _matrix[i, j] = Minimum(_matrix[i - 1, j] + 1, _matrix[i, j - 1] + 1, _matrix[i - 1, j - 1] + intCost);
                    _computeTimes++;
                }
            }
            //结束时间
            _endTime = DateTime.Now;
            //相似率 移动次数小于最长的字符串长度的20%算同一题
            int intLength = _row > _column ? _row : _column;

            _result.Rate = (1 - (decimal)_matrix[_row - 1, _column - 1] / intLength);
            _result.UseTime = (_endTime - _beginTime).ToString();
            _result.ComputeTimes = _computeTimes.ToString();
            _result.Difference = _matrix[_row - 1, _column - 1];
        }


        /// <summary>
        /// 计算相似度（不记录比较时间）
        /// </summary>
        public void SpeedyCompute()
        {
            //开始时间
            //_BeginTime = DateTime.Now;
            //初始化矩阵的第一行和第一列
            InitMatrix();
            for (int i = 1; i < _row; i++)
            {
                for (int j = 1; j < _column; j++)
                {
                    var intCost = _arrChar1[i - 1] == _arrChar2[j - 1] ? 0 : 1;
                    //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                    //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                    _matrix[i, j] = Minimum(_matrix[i - 1, j] + 1, _matrix[i, j - 1] + 1, _matrix[i - 1, j - 1] + intCost);
                    _computeTimes++;
                }
            }
            //结束时间
            //_EndTime = DateTime.Now;
            //相似率 移动次数小于最长的字符串长度的20%算同一题
            int intLength = _row > _column ? _row : _column;

            _result.Rate = (1 - (decimal)_matrix[_row - 1, _column - 1] / intLength);
            // _Result.UseTime = (_EndTime - _BeginTime).ToString();
            _result.ComputeTimes = _computeTimes.ToString();
            _result.Difference = _matrix[_row - 1, _column - 1];
        }
        /// <summary>
        /// 计算相似度
        /// </summary>
        /// <param name="str1">字符串1</param>
        /// <param name="str2">字符串2</param>
        public void Compute(string str1, string str2)
        {
            StringComputeInit(str1, str2);
            Compute();
        }

        /// <summary>
        /// 计算相似度
        /// </summary>
        /// <param name="str1">字符串1</param>
        /// <param name="str2">字符串2</param>
        public void SpeedyCompute(string str1, string str2)
        {
            StringComputeInit(str1, str2);
            SpeedyCompute();
        }
        /// <summary>
        /// 初始化矩阵的第一行和第一列
        /// </summary>
        private void InitMatrix()
        {
            for (int i = 0; i < _column; i++)
            {
                _matrix[0, i] = i;
            }
            for (int i = 0; i < _row; i++)
            {
                _matrix[i, 0] = i;
            }
        }
        /// <summary>
        /// 取三个数中的最小值
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        private int Minimum(int first, int second, int third)
        {
            int intMin = first;
            if (second < intMin)
            {
                intMin = second;
            }
            if (third < intMin)
            {
                intMin = third;
            }
            return intMin;
        }
        #endregion
    }
    /// <summary>
    /// 计算结果
    /// </summary>
    public struct Result
    {
        /// <summary>
        /// 相似度
        /// </summary>
        public decimal Rate;
        /// <summary>
        /// 对比次数
        /// </summary>
        public string ComputeTimes;
        /// <summary>
        /// 使用时间
        /// </summary>
        public string UseTime;
        /// <summary>
        /// 差异
        /// </summary>
        public int Difference;
    }
}