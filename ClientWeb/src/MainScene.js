class MainScene extends cc.Scene {
    constructor() {
        super();
        let messageLabel = creator.createLabel("", 30, cc.color(255, 20, 20));
        let showMessageTID;
        //主界面层
        let mainUI = new ccui.Widget();
        let playOnlineButton = new ccui.Button(res.PlayOnlineButton_png);
        const background = new cc.Sprite(res.MainSceneBG_png);
        background.x = size.width / 2;
        background.y = size.height / 2;
        this.addChild(background);

        const mainTitle = new cc.Sprite(res.Title_png);
        mainTitle.x = size.width / 2;
        mainTitle.y = size.height / 2 + 250;
        mainTitle.scale = 0.8;
        this.addChild(mainTitle);

        let scrollableLayer = new cc.Layer();
        this.addChild(scrollableLayer);

        //登录界面
        const loginUI = new ccui.Widget();
        scrollableLayer.addChild(loginUI);
        const titleLabel = creator.createLabel(txt.mainScene.loginTitle, 40);
        titleLabel.setPosition(size.width / 2, size.height / 2 + 100);
        loginUI.addChild(titleLabel);

        const nameBox = creator.createEditBox(txt.mainScene.enterName, cc.size(500, 60));
        const lastName = storage.getItem("name");
        if (lastName) {
            nameBox.setString(lastName);
        }
        nameBox.setPosition(size.width / 2, size.height / 2 + 20);
        loginUI.addChild(nameBox);

        function moveToMainUI() {
            messageLabel.opacity = 0;
            clearTimeout(showMessageTID);
            messageLabel.stopAllActions();
            loginUI.enabled = false;
            mainUI.enabled = true;
            playOnlineButton.enabled = !player.guest;
            const moveAction = cc.moveTo(1, cc.p(-size.width, 0)).easing(cc.easeExponentialInOut());
            scrollableLayer.runAction(moveAction);
            updatePlayerLabel();
        }

        const loginButton = creator.createButton(txt.mainScene.login, cc.size(200, 50), function () {
            const name = nameBox.getString();
            if (name.length === 0) {
                showMessage(txt.mainScene.emptyName);
            } else if (name.length > 15) {
                showMessage(txt.mainScene.nameTooLong);
            } else {
                loginUI.enabled = false;
                player.login(nameBox.getString(), function () {
                    storage.setItem("name", name);
                    moveToMainUI();
                }, function (error) {
                    loginUI.enabled = true;
                    showMessage(error);
                });
            }
        });

        loginButton.setPosition(size.width / 2, size.height / 2 - 60);
        loginUI.addChild(loginButton);

        let guestButton = creator.createButton(txt.mainScene.loginAsGuest, cc.size(200, 50), function () {
            player.loginAsGuest();
            moveToMainUI();
        });
        guestButton.setPosition(size.width / 2, size.height / 2 - 140);
        loginUI.addChild(guestButton);

        messageLabel.setPosition(size.width / 2, size.height / 2 - 200);
        messageLabel.opacity = 0;
        this.addChild(messageLabel);


        function showMessage(text) {
            messageLabel.string = text;
            messageLabel.opacity = 255;
            clearTimeout(showMessageTID);
            messageLabel.stopAllActions();
            showMessageTID = setTimeout(function () {
                messageLabel.runAction(cc.fadeOut(1));
            }, 2000);
        }
        mainUI.x = size.width;
        scrollableLayer.addChild(mainUI);

        playOnlineButton.setPosition(size.width / 2, size.height / 2 + 100);
        playOnlineButton.addClickEventListener(function () {
            cc.director.runScene(new OnlineGameScene());
        });
        mainUI.addChild(playOnlineButton);

        const playWithAIButton = creator.createButton(txt.mainScene.playWithAI, cc.size(250, 50), function () {
            cc.director.runScene(new AIGameScene());
        });
        playWithAIButton.setPosition(size.width / 2, size.height / 2 - 20);
        mainUI.addChild(playWithAIButton);

        const playDoubleButton = creator.createButton(txt.mainScene.playDouble, cc.size(250, 50), function () {
            cc.director.runScene(new DoubleGameScene());
        });
        playDoubleButton.setPosition(size.width / 2, size.height / 2 - 100);
        mainUI.addChild(playDoubleButton);

        const logoutButton = creator.createButton(txt.mainScene.logout, cc.size(150, 50), function () {
            player.logout();
            loginUI.enabled = true;
            loginUI.visible = true;
            mainUI.enabled = false;
            const moveAction = cc.moveTo(1, cc.p(0, 0)).easing(cc.easeExponentialInOut());
            scrollableLayer.runAction(moveAction);
        });
        logoutButton.setPosition(size.width / 2, size.height / 2 - 250);
        mainUI.addChild(logoutButton);

        const playerLabel = creator.createLabel("", 25);
        mainUI.addChild(playerLabel);

        function updatePlayerLabel() {
            playerLabel.string = player.name;
            playerLabel.setPosition(playerLabel.width / 2 + 20, size.height - playerLabel.height / 2 - 20);
        }

        if (player.logged) {
            loginUI.enabled = false;
            loginUI.visible = false;
            updatePlayerLabel();
            scrollableLayer.x = -size.width;
            playOnlineButton.enabled = !player.guest;
        } else {
            mainUI.enabled = false;
        }

        //固定界面
        const optionButton = new ccui.Button(res.OptionButtonN_png, res.OptionButtonS_png);
        optionButton.setPosition(optionButton.width / 2 + 10, optionButton.height / 2 + 10);
        optionButton.addClickEventListener(function () {
            cc.director.runScene(new OptionScene());
        });
        this.addChild(optionButton);

        const homepageButton = creator.createButton(txt.mainScene.homepage, cc.size(200, 50), function () {
            cc.sys.openURL("http://www.infinideas.org");
        });
        homepageButton.setPosition(size.width - homepageButton.width / 2 - 10, homepageButton.height / 2 + 20);
        this.addChild(homepageButton);

        const tutorialButton = creator.createButton(txt.mainScene.tutorial, cc.size(150, 50), function () {
            cc.director.runScene(new TutorialGameScene());
        });
        tutorialButton.setPosition(size.width - tutorialButton.width / 2 - 10, tutorialButton.height / 2 + homepageButton.height + 40);
        this.addChild(tutorialButton);
    }
}
