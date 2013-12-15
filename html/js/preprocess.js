define('bluebird', function () {
    return require('bluebird');
});

define('Promise', function () {
    return require('bluebird');
});

define('require', function () {
    return require;
});

define('$', function (require) {
    require('./zepto.js');
    var $ = window.Zepto;
    var Promise = require('require')('bluebird');
    $.ajaxAsync = function (config) {
        var defer = Promise.defer();
        $.ajax($.extend(config, {
            success: function (data, status, xhr) {
                defer.resolve(data);
            },
            error: function (xhr, errorType, error) {
                defer.reject(error);
            }
        }));
        return defer.promise;
    };
    return $;
});

define('sammy', function (require) {
    require('./sammy.js');
    return window.Sammy;
});

define('ko', function (require) {
    require('./knockout-3.0.0.debug.js');
    ko.bindingHandlers.allowBindings = {
        init: function(elem, valueAccessor) {
            // Let bindings proceed as normal *only if* my value is false
            var shouldAllowBindings = ko.unwrap(valueAccessor());
            return { controlsDescendantBindings: !shouldAllowBindings };
        }
    };
    return window.ko;
});

define('css', function (_require) {
    var $ = _require("$");
    require("" + '../lib/util')
        .buildLess([
            './css/veryless.less',
            './css/veryless-plugin.less',
            './css/main.less'
        ])
        .then(function (css) {
            $('head').append($('<style type="text/css">' + css + '</style>'));
        });
});

define('template', function (_require, exports, module) {
    var Promise = _require('bluebird');
    var $ = _require('$');
    return exports = module.exports = {
        loadTemplate: function loadTemplate() {
            return Promise.resolve([].slice.call(arguments))
                .map(function (id) {
                    return $.ajaxAsync({
                        url: './view/' + id + '.html',
                        dataType: 'text'
                    }).then(function (data) {
                            $(document.body).append("<script id='" + id + "' type='text/html'>" + data + "</script>");
                            return null;
                        });
                });
        }
    }
});

define('service', function () {
    return process.mainModule.exports.init();
});

seajs.use(['css', './js/module/index.js']);
