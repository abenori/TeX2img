namespace TeX2img.Properties {


    // このクラスでは設定クラスでの特定のイベントを処理することができます:
    //  SettingChanging イベントは、設定値が変更される前に発生します。
    //  PropertyChanged イベントは、設定値が変更された後に発生します。
    //  SettingsLoaded イベントは、設定値が読み込まれた後に発生します。
    //  SettingsSaving イベントは、設定値が保存される前に発生します。
    internal sealed partial class Settings {

        public Settings() {
            // // 設定の保存と変更のイベント ハンドラーを追加するには、以下の行のコメントを解除します:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // SettingChangingEvent イベントを処理するコードをここに追加してください。
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // SettingsSaving イベントを処理するコードをここに追加してください。
        }

        protected override void OnSettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e) {
            editorFontColor["テキスト"] = new FontColor(editorNormalColorFont, editorNormalColorBack);
            editorFontColor["選択範囲"] = new FontColor(editorSelectedColorFont, editorSelectedColorBack);
            editorFontColor["コントロールシークエンス"] = new FontColor(editorCommandColorFont, editorCommandColorBack);
            editorFontColor["$"] = new FontColor(editorEquationColorFont, editorEquationColorBack);
            editorFontColor["中 / 大括弧"] = new FontColor(editorBracketColorFont, editorBracketColorBack);
            editorFontColor["コメント"] = new FontColor(editorCommentColorFont, editorCommentColorBack);
            editorFontColor["改行，EOF"] = new FontColor(editorEOFColorFont, editorNormalColorBack);
            editorFontColor["対応する括弧"] = new FontColor(editorMatchedBracketColorFont, editorMatchedBracketColorBack);
            base.OnSettingsLoaded(sender, e);
        }

        protected override void OnSettingsSaving(object sender, System.ComponentModel.CancelEventArgs e) {
            editorNormalColorFont = editorFontColor["テキスト"].Font;
            editorNormalColorBack = editorFontColor["テキスト"].Back;
            editorSelectedColorFont = editorFontColor["選択範囲"].Font;
            editorSelectedColorBack = editorFontColor["選択範囲"].Back;
            editorCommandColorFont = editorFontColor["コントロールシークエンス"].Font;
            editorCommandColorBack = editorFontColor["コントロールシークエンス"].Back;
            editorEquationColorFont = editorFontColor["$"].Font;
            editorEquationColorBack = editorFontColor["$"].Back;
            editorBracketColorFont = editorFontColor["中 / 大括弧"].Font;
            editorBracketColorBack = editorFontColor["中 / 大括弧"].Back;
            editorCommentColorFont = editorFontColor["コメント"].Font;
            editorCommentColorBack = editorFontColor["コメント"].Back;
            editorEOFColorFont = editorFontColor["改行，EOF"].Font;
            editorMatchedBracketColorFont = editorFontColor["対応する括弧"].Font;
            editorMatchedBracketColorBack = editorFontColor["対応する括弧"].Back;
            base.OnSettingsSaving(sender, e);
        }

        public override void Save() {
            if(!NoSaveSettings) base.Save();
        }

        public class FontColor {
            public System.Drawing.Color Font { get; set; }
            public System.Drawing.Color Back { get; set; }
            public FontColor() { }
            public FontColor(System.Drawing.Color f, System.Drawing.Color b) { Font = f; Back = b; }
        }

        public class FontColorCollection : System.Collections.Generic.Dictionary<string, FontColor> {
            public new FontColor this[string key] {
                get {
                    if(key == "改行，EOF" && base.ContainsKey(key)) base["改行，EOF"].Back = base["テキスト"].Back;
                    return base[key];
                }
                set {
                    base[key] = value;
                    if(key == "改行，EOF" && base.ContainsKey(key)) base["改行，EOF"].Back = base["テキスト"].Back;
                }
            }
        }
        public FontColorCollection editorFontColor = new FontColorCollection();
        public bool NoSaveSettings = false;
    }
}
