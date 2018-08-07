using Biskuits.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Biskuits.Services.LifeCycleLogStore;

namespace UnitTests.Services
{
    [TestFixture]
    class LifeCycleLogStoreTest
    {

        /// <summary>
        /// <seealso cref="LifeCycleLogStore"/>の利用例を表現したクラスです.
        /// IDisposableをインターフェースに持ちます.
        /// </summary>
        class Model : IDisposable
        {
            public readonly Guid OwnLifeCycleLogGuid;
            public readonly LifeCycleLog OwnLifeCycleLog;

            public Model()
            {
                OwnLifeCycleLog = LifeCycleLogStore.Instance.CreateLifeCycleLog(this, OwnLifeCycleLogGuid = Guid.NewGuid());
            }

            ~Model()
            {
                // アンマネージドリソースを解放します (マネージドリソースのGC回収は自動で行われます)
                Dispose(false);
                OwnLifeCycleLog.OnFinalized();
            }

            #region IDisposable
            private bool isDisposed = false;

            public void Dispose()
            {
                // マネージド・アンマネージドの両リソースを解放
                Dispose(true);
                OwnLifeCycleLog.OnDisposed();
            }

            protected virtual void Dispose(bool disposing)
            {
                // 多重にDisposeされたら 例外を投げます
                if (isDisposed)
                    throw new InvalidOperationException($"{GetType().Name}は すでにDisposeされています。");

                if (disposing)
                {
                    // マネージドリソースの解放処理
                    ;

                    // ファイナライズの抑制
                    GC.SuppressFinalize(this);
                    OwnLifeCycleLog.OnSuppressedFinalize();
                }

                // アンマネージドリソースの解放処理
                ;

                // Dispose済みであることを記憶します
                isDisposed = true;
                OwnLifeCycleLog.OnInnnerDisposed();
            }
            #endregion
        }

        [Test]
        public void Test()
        {
            var store = LifeCycleLogStore.Instance;
            Model model_01 = null;
            Model model_02 = null;

            Func<string, string[]> convertLogToStates = (log) =>
            {
                var splitOpt = StringSplitOptions.RemoveEmptyEntries;
                return log
                    .Split(new string[] { Environment.NewLine }, splitOpt)
                    .Skip(1)
                    .Reverse().Skip(1).Reverse()
                    .Select(s =>
                    {
                        return s
                        .Replace(" +", " ")
                        .Split(new char[] { ' ' }, splitOpt)[1];
                    })
                    .ToArray();
            };

            // === ログを確認する (オブジェクト作成直後)

            model_01 = new Model();
            model_02 = new Model();
            Console.WriteLine($"オブジェクトを作成しました。");

            store.UpdateLogsStrings();
            Console.WriteLine(store.UpdatedLogsString);

            Assert.That(convertLogToStates(store.UpdatedLogsString), Is.EqualTo(new string[] {
              // CDSIFA
                "C----A",
                "C----A",
            }));


            // === ログを確認する (Dispose直後)

            model_01.Dispose();
            Console.WriteLine($"Disposeしました。");

            store.UpdateLogsStrings();
            Console.WriteLine(store.UpdatedLogsString);

            Assert.That(convertLogToStates(store.UpdatedLogsString), Is.EqualTo(new string[] {
              // CDSIFA
                "CDIS-A",
            }));


            // === ログを確認する (オブジェクトのGC回収直後)

            model_01 = null;
            model_02 = null;
            Task.Delay(TimeSpan.FromMilliseconds(10));
            store.GCCollect().Wait();
            Task.Delay(TimeSpan.FromMilliseconds(10));
            store.GCCollect().Wait();
            Console.WriteLine($"オブジェクトをGC回収しました。");

            store.UpdateLogsStrings();
            Console.WriteLine(store.UpdatedLogsString);

            Assert.That(convertLogToStates(store.UpdatedLogsString), Is.EqualTo(new string[] {
              // CDSIFA
                "CDIS-D",
                "C-I-FD",
            }));
        }
    }
}
