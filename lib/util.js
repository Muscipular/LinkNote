"use strict";
var Promise = require('bluebird');
var fs = Promise.promisifyAll(require('fs'));
var render = require('less').render;
var renderLess = Promise.promisify(render);

//Promise.longStackTraces();
var buildLess = (function () {
    var args = Array.prototype.slice.call(arguments);
    if (args.length === 1 && Array.isArray(args[0])) {
        args = args[0];
    }
    return Promise.map(args, (function (v) {
            return fs.readFileAsync(process.cwd() + '/html/' + v, {encoding: 'utf8'})
                .then(function (lessData) {
                    return renderLess(lessData, {compress: true});
                });
        }))
        .then(function (result) {
            return result.join(' ');
        })
});

exports.buildLess = buildLess;
