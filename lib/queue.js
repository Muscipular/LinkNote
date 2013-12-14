"use strict";

var queueList = [];
var defaultJob = {
    run: function (cb) {
        cb(true);
    },
    delay: 0,
    loop: 0,
    timeout: 0,
    a: 0
};

var queue = {
    push: function (job) {

    }
};


exports.module = queue;