//Example: txt.a.b
let txt = {};
const lang = new class {
    constructor() {
        this.autoLang = true;
        this.currentLang = null;
        this.langs = [];
        this.autoLangList = {}; //这里存的是langs里的序号
    }

    init() {
        let sLang = storage.getItem("lang");
        if (!sLang || sLang === "auto")
            lang.loadAutoLang();
        else
            lang.loadLang(sLang);
    }

    load (langName, cb) {
        const path = "res/lang/" + langName + ".json";
        cc.loader.loadJson(path, function (err, data) {
            if (err) return cc.log("load lang file failed " + err);
            txt = data;
            player.resetName();
            if (cb) cb();
        });
    }

    loadLang (langName, cb) {
        lang.load(langName, function () {
            lang.autoLang = false;
            lang.currentLang = langName;
            storage.setItem("lang", langName);
            if (cb) cb();
        });
    }

    loadAutoLang (cb) {
        const index = lang.autoLangList[cc.sys.language] || 0;
        lang.load(lang.langs[index][1], function () {
            lang.autoLang = true;
            storage.setItem("lang", "auto");
            if (cb) cb();
        });
    }
};

lang.langs[0] = ["English(US)", "en_US"];
lang.langs[1] = ["简体中文", "zh_CN"];
lang.langs[2] = ["正體中文", "zh_TW"];

lang.autoLangList[cc.sys.LANGUAGE_ENGLISH] = 0;
lang.autoLangList[cc.sys.LANGUAGE_CHINESE] = 1;
