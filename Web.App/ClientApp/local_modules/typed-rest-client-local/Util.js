// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
import * as url from 'url';
import * as path from 'path';
/**
 * creates an url from a request url and optional base url (http://server:8080)
 * @param {string} resource - a fully qualified url or relative path
 * @param {string} baseUrl - an optional baseUrl (http://server:8080)
 * @return {string} - resultant url
 */
export function getUrl(resource, baseUrl) {
    if (!baseUrl) {
        return resource;
    }
    if (!resource) {
        return baseUrl;
    }
    var base = url.parse(baseUrl);
    // resource (specific per request) eliments take priority
    var resultantUrl = url.parse(resource);
    resultantUrl.protocol = resultantUrl.protocol || base.protocol;
    resultantUrl.auth = resultantUrl.auth || base.auth;
    resultantUrl.host = resultantUrl.host || base.host;
    resultantUrl.pathname = path.posix.resolve(base.pathname, resultantUrl.pathname);
    var res = url.format(resultantUrl);
    return res;
}