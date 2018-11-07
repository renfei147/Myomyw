class LangOptionScene extends MenuScene {
    constructor() {
        super(txt.options.lang, function () {
            cc.director.runScene(new OptionScene());
        });

        let list = new ccui.ListView();
        list.setDirection(ccui.ScrollView.DIR_VERTICAL);
        list.setGravity(ccui.ListView.GRAVITY_CENTER_HORIZONTAL);
        list.setItemsMargin(20);
        list.setBounceEnabled(true);
        list.setContentSize(size.width - 100, size.height - 120);
        list.setAnchorPoint(0.5, 0.5);
        list.setPosition(size.width / 2, size.height / 2 - 10);
        this.addChild(list);

        //在这里bright反而是不高亮(别问我为啥这么做)
        let highlightedButton;
        const title = this.getChildByName("title");

        function onTouchedButton(button) {
            if (button === highlightedButton) return;
            highlightedButton.bright = true;
            highlightedButton = button;
            button.bright = false;

            if (button.tag === -1) {
                lang.loadAutoLang(function () {
                    title.string = txt.options.lang;
                    autoLangButton.titleText = txt.options.autoLang;
                });
            }
            else {
                lang.loadLang(lang.langs[button.tag][1], function () {
                    title.string = txt.options.lang;
                    autoLangButton.titleText = txt.options.autoLang;
                });
            }
        }

        let autoLangButton = creator.createButton(txt.options.autoLang, cc.size(600, 60), onTouchedButton);
        autoLangButton.tag = -1;
        autoLangButton.loadTextureDisabled(res.HighlightedButton_png);
        if (lang.autoLang) {
            highlightedButton = autoLangButton;
            autoLangButton.bright = false;
        }
        list.addChild(autoLangButton);

        for (let i = 0; i < lang.langs.length; i++) {
            let button = creator.createButton(lang.langs[i][0], cc.size(600, 60), onTouchedButton);
            button.tag = i;
            button.loadTextureDisabled(res.HighlightedButton_png);
            if (!lang.autoLang && lang.langs[i][1] === lang.currentLang) {
                highlightedButton = button;
                button.bright = false;
            }
            list.addChild(button);
        }
    }
}
