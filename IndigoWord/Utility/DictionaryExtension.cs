using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NUnit.Framework;

namespace IndigoWord.Utility
{
    static class DictionaryExtension
    {
        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> match)
        {
            foreach (var key in dictionary.Keys.ToArray())
            {
                if (match(key, dictionary[key]))
                {
                    dictionary.Remove(key);
                }
            }
        }

        public static TKey GetKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            return dictionary.Single(pair => ReferenceEquals(pair.Value, value)).Key;
        }

        /*
         * Insert the given insertElements from the given index
         * And shift right the rest elements.
         * like:
         * [0, "a"], [1, "b"], [2, "c"], [3, "d"], [4, "e"]
         * After InsertAndShift(1, {"x", "y"})
         * [0, "a"], [1, "x"], [2, "y"], [3, "b"], [4, "c"], [5, "d"], [6, "e"]
         */
        public static void InsertAndShift<TValue>(this IDictionary<int, TValue> dictionary, int index, IList<TValue> insertElements)
        {
            var insertSize = insertElements.Count;
            var remainingSize = dictionary.Count - index;
            var newSize = dictionary.Count + insertSize;
            var oldSize = dictionary.Count;

            for (int i = 0; i < remainingSize; i++)
            {
                //shift from the end
                dictionary[newSize - 1 - i] = dictionary[oldSize - 1 - i];
            }

            //insert
            int insertPos = 0;
            for (int i = 0; i < insertSize; i++, insertPos++)
            {
                dictionary[i + index] = insertElements[insertPos];
            }
        }

        /*
         * Remove from the given index and the count is the given size
         * And shift the rest elements to the given index.
         * like:
         * [0, "a"], [1, "b"], [2, "c"], [3, "d"], [4, "e"]
         * After RemoveAndShift(1,2)
         * [0, "a"], [1, "d"], [2, "e"]
         * 
         */
        public static void RemoveAndShift<TValue>(this IDictionary<int, TValue> dictionary, int index, int size)
        {
            for (int i = 0; i < dictionary.Count - index - size; i++)
            {
                var key = index + i;
                dictionary[key] = dictionary[key + size];
            }

            var sum = dictionary.Count;
            for (int i = 0; i < size; i++)
            {
                dictionary.Remove(sum - 1 - i);
            }
        }
    }

    #region Tests

    [TestFixture]
    internal class DictionaryExtensionTest
    {

        private readonly string[] _data =
        {
            "a",
            "b",
            "c",
            "d",
            "e"
        };

        private Dictionary<int, string> MyDictionary { get; set; }
            
        [SetUp]
        public void Init()
        {
            MyDictionary = new Dictionary<int, string>();

            int n = 0;
            foreach (var str in _data)
            {
                MyDictionary.Add(n, str);
                n++;
            }
        }

        [Test]
        public void InsertAndShift_1()
        {
            MyDictionary.InsertAndShift(0, new [] {"x"});

            Assert.AreEqual(6, MyDictionary.Count);
            Assert.AreEqual("x", MyDictionary[0]);
            Assert.AreEqual("a", MyDictionary[1]);
            Assert.AreEqual("b", MyDictionary[2]);
            Assert.AreEqual("c", MyDictionary[3]);
            Assert.AreEqual("d", MyDictionary[4]);
            Assert.AreEqual("e", MyDictionary[5]);
        }

        [Test]
        public void InsertAndShift_2()
        {
            MyDictionary.InsertAndShift(1, new[] { "x" });

            Assert.AreEqual(6, MyDictionary.Count);
            Assert.AreEqual("a", MyDictionary[0]);
            Assert.AreEqual("x", MyDictionary[1]);
            Assert.AreEqual("b", MyDictionary[2]);
            Assert.AreEqual("c", MyDictionary[3]);
            Assert.AreEqual("d", MyDictionary[4]);
            Assert.AreEqual("e", MyDictionary[5]);
        }

        [Test]
        public void InsertAndShift_3()
        {
            MyDictionary.InsertAndShift(5, new[] { "x" });

            Assert.AreEqual(6, MyDictionary.Count);
            Assert.AreEqual("a", MyDictionary[0]);
            Assert.AreEqual("b", MyDictionary[1]);
            Assert.AreEqual("c", MyDictionary[2]);
            Assert.AreEqual("d", MyDictionary[3]);
            Assert.AreEqual("e", MyDictionary[4]);
            Assert.AreEqual("x", MyDictionary[5]);
        }

        [Test]
        public void InsertAndShift_4()
        {
            MyDictionary.InsertAndShift(2, new[] { "x", "y" });

            Assert.AreEqual(7, MyDictionary.Count);
            Assert.AreEqual("a", MyDictionary[0]);
            Assert.AreEqual("b", MyDictionary[1]);
            Assert.AreEqual("x", MyDictionary[2]);
            Assert.AreEqual("y", MyDictionary[3]);
            Assert.AreEqual("c", MyDictionary[4]);
            Assert.AreEqual("d", MyDictionary[5]);
            Assert.AreEqual("e", MyDictionary[6]);
        }

        [Test]
        public void RemoveAndShift_1()
        {
            MyDictionary.RemoveAndShift(1, 1);

            Assert.AreEqual(4, MyDictionary.Count);
            Assert.AreEqual("a", MyDictionary[0]);
            Assert.AreEqual("c", MyDictionary[1]);
            Assert.AreEqual("d", MyDictionary[2]);
            Assert.AreEqual("e", MyDictionary[3]);
        }

        [Test]
        public void RemoveAndShift_2()
        {
            MyDictionary.RemoveAndShift(2, 3);

            Assert.AreEqual(2, MyDictionary.Count);
            Assert.AreEqual("a", MyDictionary[0]);
            Assert.AreEqual("b", MyDictionary[1]);
        }

        [Test]
        public void RemoveAndShift_3()
        {
            MyDictionary.RemoveAndShift(1, 2);

            Assert.AreEqual(3, MyDictionary.Count);
            Assert.AreEqual("a", MyDictionary[0]);
            Assert.AreEqual("d", MyDictionary[1]);
            Assert.AreEqual("e", MyDictionary[2]);
        }

        [Test]
        public void RemoveAndShift_4()
        {
            MyDictionary.RemoveAndShift(0, 5);

            Assert.AreEqual(0, MyDictionary.Count);
        }
    }

    #endregion
}
