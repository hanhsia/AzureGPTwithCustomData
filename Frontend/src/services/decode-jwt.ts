export function decodeToken(token: string = '') {
    let parts = token.split('.');
    let headers = parts[0];
    let payload = parts.length > 1 ? parts[1] : null;
    if (payload != null) {

        // determine the padding characters required for the base64 string
        let padding: string = "=".repeat((4 - (payload.length % 4)) % 4);
        // convert the base64url string to a base64 string
        let base64: string =
            payload.replace("-", "+").replace("_", "/") + padding;
        // decode and parse to json
        let decoded = JSON.parse(atob(base64));

        return payload ? decoded : ""
        
    }
    return null
}

export function isTokenExpired(token: string): boolean {
    const decodedToken: any = decodeToken(token);
    let result: boolean = true;

    if (decodedToken && decodedToken.exp) {
        const expirationDate: Date = new Date(0);
        expirationDate.setUTCSeconds(decodedToken.exp); // sets the expiration seconds
        // compare the expiration time and the current time
        result = expirationDate.valueOf() < new Date().valueOf();
    }

    return result;
}