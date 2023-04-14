//export default function authHeader(token:string) {
//    let parsedToken = null;
//    if (token)
//        parsedToken = JSON.parse(token);

//    if (parsedToken && parsedToken.jwt) {
//        return { Authorization: 'Bearer ' + parsedToken.jwt }; 
//    } else {
//        return { Authorization: '' }; 
//    }
//}

export default function authHeader(token: string) {
    let parsedToken = null;
    if (token) { 
        return { Authorization: 'Bearer ' + token };
     } 
     else {
        return { Authorization: '' };
    }
}