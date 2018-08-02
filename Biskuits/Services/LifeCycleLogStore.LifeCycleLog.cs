using System;
using System.Diagnostics;
using System.Text;

namespace Biskuits.Services
{
    public partial class LifeCycleLogStore
    {
        /// <summary>
        /// インスタンスのライフサイクルログ
        /// </summary>
        public class LifeCycleLog
        {

            /// 定数 =============================================

            static readonly string StringHeader = $"{nameof(LifeCycleLog)}:";


            /// 共通機能 =========================================

            /// ターゲットの情報
            public WeakReference<object> TargetWeakReference { get; }
            public string Namespace { get; }
            public Type TargetType { get; }
            public Guid Guid { get; }

            /// コンストラクタ
            public LifeCycleLog(object target, Guid guid)
            {
                // trackResurrection: true としているのは ファイナライズで利用することがあるため
                TargetWeakReference = new WeakReference<object>(target, trackResurrection: true);
                Namespace = target.GetType().Namespace;
                TargetType = target.GetType();
                Guid = guid;

                IsConstructed = true;
                PostProcOnXXX("OnConstructed");
            }


            /// ターゲットの状態を記録する機能 ===================

            /// ターゲットの状態
            public bool IsConstructed { get; private set; }
            public bool IsDisposed { get; private set; }
            public bool IsInnerDisposed { get; private set; }
            public bool IsSuppressedFinalize { get; private set; }
            public bool IsFinalized { get; private set; }
            public bool IsAlive
            {
                get { return TargetWeakReference.TryGetTarget(out _); }
            }

            /// <summary>
            /// ターゲットがDisposeされた最後に呼び出す
            /// </summary>
            public void OnDisposed()
            {
                IsDisposed = true;
                PostProcOnXXX("OnDisposed");
            }
            /// <summary>
            /// ターゲットのDisposeの実処理後に呼び出す
            /// </summary>
            public void OnInnnerDisposed()
            {
                IsInnerDisposed = true;
                PostProcOnXXX("OnInnerDisposed");
            }
            /// <summary>
            /// ターゲットがファイナライズ抑制された後に呼び出す
            /// </summary>
            public void OnSuppressedFinalize()
            {
                IsSuppressedFinalize = true;
                PostProcOnXXX("OnSuppressedFinalize");
            }
            /// <summary>
            /// ターゲットがファイナライズされた最後に呼び出す
            /// </summary>
            public void OnFinalized()
            {
                IsFinalized = true;
                PostProcOnXXX("OnFinalized");
            }
            /// <summary>
            /// OnXXXの共通処理
            /// </summary>
            void PostProcOnXXX(string xxxName)
            {
                IsUpdated = true;

                object instance;
                if (TargetWeakReference.TryGetTarget(out instance))
                    Debug.WriteLine($"{LifeCycleLog.StringHeader} {TargetType.Name}({Guid}).{xxxName}");
            }


            /// ログを文字列化する機能============================

            /// ターゲットの更新の有無
            bool IsUpdated
            {
                get
                {
                    /// <see cref="IsAlive"/> の更新を検知する
                    if (IsAlive != IsAliveKeeper)
                    {
                        IsAliveKeeper = IsAlive;
                        _IsUpdated = true;
                    }
                    return _IsUpdated;
                }
                set { _IsUpdated = value; }
            }
            bool _IsUpdated;
            bool IsAliveKeeper = true;

            /// <summary>
            /// ログの文字列化を <paramref name="sb"/> に付加する
            /// </summary>
            public StringBuilder AppendLogString(StringBuilder sb, string header = null)
            {
                header = header ?? LifeCycleLog.StringHeader;
                var none = "-";

                // Header
                sb.Append(header).Append(" ");
                // States
                sb.Append(IsConstructed ? "C" : none);
                sb.Append(IsDisposed ? "D" : none);
                sb.Append(IsInnerDisposed ? "I" : none);
                sb.Append(IsSuppressedFinalize ? "S" : none);
                sb.Append(IsFinalized ? "F" : none);
                sb.Append(IsAlive ? "A" : "D");
                sb.Append(" ");
                // Info
                sb.Append(string.Format("{0, -20}", TargetType.Name)).Append(" ");
                sb.Append(Guid).Append(" ");
                sb.Append(Namespace).Append(" ");

                return sb;
            }
            /// <summary>
            /// ログの文字列化を <paramref name="sb"/> に付加する（ただし ログが更新された場合のみ）
            /// </summary>
            public StringBuilder AppendUpdatedLogString(StringBuilder sb, string header = null, string footer = "")
            {
                // 更新があれば付加する
                if (IsUpdated)
                {
                    AppendLogString(sb, header).Append(footer);
                    IsUpdated = false;
                }
                return sb;
            }

        }/// end of class
    }
}
