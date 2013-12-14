"use strict";
var util = require('util');

var defineError = function (base) {
    var Type = function () {
        Type.super_.apply(this, [].slice.call(arguments));
    };

    return util.inherits(Type, base || Error);
};

exports.CryptoError = defineError();

