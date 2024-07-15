using System.Collections.Generic;

namespace DefaultNamespace
{
    public class Test
    {
        /// <summary>
        /// 实现快速排序
        /// </summary>
        /// <param name="elements"></param>
        List<int> SortQuick(List<int> elements)
        {
            if (elements.Count <= 1)
            {
                return elements;
            }

            int pivot = elements[0];
            List<int> left = new List<int>();
            List<int> right = new List<int>();

            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i] < pivot)
                {
                    left.Add(elements[i]);
                }
                else
                {
                    right.Add(elements[i]);
                }
            }

            List<int> result = new List<int>();
            result.AddRange(SortQuick(left));
            result.Add(pivot);
            result.AddRange(SortQuick(right));

            return result;
        }
        
        // 冒泡排序
        int[] SortBubble(int[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                for (int j = 0; j < elements.Length - i - 1; j++)
                {
                    if (elements[j] > elements[j + 1])
                    {
                        int temp = elements[j];
                        elements[j] = elements[j + 1];
                        elements[j + 1] = temp;
                    }
                }
            }

            return elements;
        }
        
        // 选择排序
        int[] SortSelect(int[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < elements.Length; j++)
                {
                    if (elements[j] < elements[minIndex])
                    {
                        minIndex = j;
                    }
                }

                int temp = elements[i];
                elements[i] = elements[minIndex];
                elements[minIndex] = temp;
            }

            return elements;
        }
        
        // 二分法查找
        public int BinarySearch(int[] nums, int target) {
            int left = 0, right = nums.Length - 1;
            while (left <= right) {
                int mid = left + (right - left) / 2;
                if (nums[mid] == target) return mid;
                else if (nums[mid] < target) left = mid + 1;
                else right = mid - 1;
            }
            return -1;
        }

    }
}