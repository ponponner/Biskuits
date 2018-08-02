using Biskuits.Helpers.Observables;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biskuits.Services
{
    /// <summary>
    /// インスタンスのライフサイクルを記憶するためのシステム。
    /// メモリリーク、<c>Dispose</c>、ファイナライズなどの
    /// ライフサイクルに関する処理の漏れを検知するために利用する。
    /// </summary>
    public partial class LifeCycleLogStore : ObservableObject
    {

        /// 定数 =============================================

        static readonly string StringHeader = $"{nameof(LifeCycleLogStore)}:";


        /// シングルトン =====================================

        public static LifeCycleLogStore Instance
        {
            get { return _Instance ?? (_Instance = new LifeCycleLogStore()); }
        }
        static LifeCycleLogStore _Instance;


        /// 共通機能 =========================================

        LifeCycleLogStore()
        {
            LifeCycleLogs = new ObservableRangeCollection<LifeCycleLog>();
        }


        /// ユーティリティ機能 ===============================

        /// <summary>
        /// <paramref name="milliseconds"/> 経過後、ガベージコレクションを呼ぶ
        /// </summary>
        public async Task GCCollect(double milliseconds = 0)
        {
            // GCに回収されるよう 少し時間をおいて呼び出す
            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));

            Debug.WriteLine($"{StringHeader} GC.Collect()");
            GC.Collect();

            Debug.WriteLine($"{StringHeader} GC.WaitForPendingFinalizers()");
            GC.WaitForPendingFinalizers();
        }


        /// ログの管理機能 ===================================

        public ObservableRangeCollection<LifeCycleLog> LifeCycleLogs { get; }

        /// <summary>
        /// ターゲットの <see cref="LifeCycleLog"/> を作成し、管理対象とする
        /// </summary>
        public void CreateLifeCycleLog(object target, Guid guid)
        {
            LifeCycleLogs.Add(new LifeCycleLog(target, guid));
        }
        /// <summary>
        /// <paramref name="guid"/> が一致する <see cref="LifeCycleLog"/> を <see cref="LifeCycleLogs"/> から取得する
        /// </summary>
        public LifeCycleLog Find(Guid guid)
        {
            var log = LifeCycleLogs.FirstOrDefault(m => m.Guid.Equals(guid)) as LifeCycleLog;
            if (log == null)
                throw new ArgumentException($"{typeof(LifeCycleLog).Name}{{Guid: {guid}}} が見つかりませんでした。");
            return log;
        }


        /// ログの文字列化機能 ===============================

        public string UpdatedLogsString
        {
            get { return _UpdatedLogsString; }
            private set { SetProperty(ref _UpdatedLogsString, value); }
        }
        string _UpdatedLogsString;
        public string AllLogsString
        {
            get { return _AllLogsString; }
            private set { SetProperty(ref _AllLogsString, value); }
        }
        string _AllLogsString;

        /// <summary>
        /// <see cref="UpdatedLogsString"/> と <see cref="AllLogsString"/> を更新します
        /// </summary>
        public void UpdateLogsStrings()
        {
            UpdatedLogsString = ToLogsString(true, StringHeader);
            AllLogsString = ToLogsString(false, StringHeader);

            Debug.WriteLine(UpdatedLogsString);
            Debug.WriteLine(AllLogsString);
        }

        /// <summary>
        /// <paramref name="logs"/> を文字列化する
        /// </summary>
        /// <param name="isOnlyNews"><c>true</c>であれば更新のあるログのみ、<c>false</c>であれば全てのログを文字列化する</param>
        public string ToLogsString(bool isOnlyNews = false, string header = null)
        {
            var newLine = Environment.NewLine;

            // 文字列の一部を作成
            header = header ?? LifeCycleLogStore.StringHeader;
            var modeString = isOnlyNews ? "News" : "All";
            var beginString = $"{header} {modeString} Begin =========={newLine}";
            var endString = $"{header} {modeString} End =========={newLine}";

            // 文字列の作成
            var sb = new StringBuilder();
            sb.Append(beginString);
            foreach (var log in this.LifeCycleLogs)
            {
                if (isOnlyNews)
                    log.AppendUpdatedLogString(sb, header, newLine);
                else
                    log.AppendLogString(sb, header).Append(newLine);
            }
            sb.Append(endString);

            return sb.ToString();
        }

    }/// end of class
}
