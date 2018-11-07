class creator {
    static init() {
        switch (cc.sys.os) {
            case cc.sys.OS_WINDOWS:
                switch (cc.sys.language) {
                    case cc.sys.LANGUAGE_CHINESE:
                        creator.normalFont = "Microsoft Yahei";
                        break;
                    default:
                        creator.normalFont = "Arial";
                        break;
                }
                break;
            default:
                creator.normalFont = "";
                break;
        }
    }

    static createLabel(text, size, color) {
        let label = new cc.LabelTTF(text, creator.normalFont, size);
        label.color = color ? color : cc.color(0, 0, 0);
        return label;
    }

    static createEditBox(placeHolder, size) {
        const editBox = new cc.EditBox(size, new cc.Scale9Sprite(res.EditBox_png));
        editBox.setFont(creator.normalFont, 25);
        editBox.setFontColor(cc.color(0, 0, 0));
        editBox.setPlaceHolder(placeHolder);
        editBox.setPlaceholderFont(creator.normalFont, 25);
        editBox.setPlaceholderFontColor(cc.color(100, 100, 100));
        return editBox;
    }

    static createButton(text, size, event) {
        const button = new ccui.Button(res.Button_png);
        button.titleFontName = creator.normalFont;
        button.titleFontSize = 25;
        button.titleText = text;
        button.setScale9Enabled(true);
        button.setContentSize(size);
        button.addClickEventListener(event);
        return button;
    }

    static createCheckBoxButton(text, size, selected, event) {
        const backgroundButton = creator.createButton(text, size, function () {
        });
        backgroundButton.loadTexturePressed(res.Button_png);//防止出现按下动画

        const checkBox = new ccui.CheckBox(res.CheckBoxNormal_png, res.CheckBoxNormal_png,
            res.CheckBoxActive_png, res.CheckBoxNormal_png, res.CheckBoxActive_png);
        checkBox.setPosition(backgroundButton.width - checkBox.width / 2 - 10, backgroundButton.height / 2);
        checkBox.selected = selected;
        checkBox.addEventListener(event);
        backgroundButton.addChild(checkBox);

        return backgroundButton;
    }
}
