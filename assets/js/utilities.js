(function (doc, win) {
    /*初始化 默认宽度、字体、最小最大比例*/
    var init_w = 1920,
    init_fs = 24,
    max_scale = 1,
    min_scale = .5;
    var docEl = doc.documentElement,
    resizeEvt = 'orientationchange' in window ? 'orientationchange' : 'resize',
    recalc = function () {
        var clientWidth = docEl.clientWidth;
        if (!clientWidth) return;
        var percentage = clientWidth / init_w;
        percentage = percentage > max_scale ? max_scale : percentage < min_scale ? min_scale : percentage;

        docEl.style.fontSize = init_fs * percentage + 'px';
    };
    if (!doc.addEventListener) return;
    win.addEventListener(resizeEvt, recalc, false);
    doc.addEventListener('DOMContentLoaded', recalc, false);
})(document, window);






// 引用公共样式
$(function () {
    //引用head相同部分
    $.ajax({
        type: 'GET',
        url: 'template/head.html',
        async: false,
        success: function (msg) { //msg随便自定义
            $('#headNav').append(msg);
        }
    })
    //引用foot相同部分
    $.ajax({
        type: 'GET',
        url: 'template/foot.html',
        async: false,
        success: function (msg) { //msg随便自定义
            $('#foot').append(msg);
        }
    })

    $("#changePicture").attr("src", "../../assets/image/log2.png");
    $(".jiantou").attr("src", "../../assets/image/jiantou2.png");

    $(window).scroll(function () {
        if ($(this).scrollTop() > 50) {
            $('#headNav').addClass('navbar-fixed-top');
            $("#changePicture").attr("src", "../../assets/image/log.png");
            $(".jiantou").attr("src", "../../assets/image/jiantou.png");

        } else {
            $('#headNav').removeClass('navbar-fixed-top');
            $("#changePicture").attr("src", "../../assets/image/log2.png");
            $(".jiantou").attr("src", "../../assets/image/jiantou2.png");
        }
    });


})