import axios from "axios";
import { SeverityLevel } from '@microsoft/applicationinsights-web';

const API_URL = process.env.REACT_APP_DOTNET_API_PATH;


const getAccessToken = async (instance: any): Promise<string | null> => {

    return "";
}

const generateDirectlineToken = async (): Promise<string | null> => {

    try {

        let response = await axios.post(
            API_URL + "/api/auth/generateDirectlineToken",
            {}            
        )
        if (response.status === 200) {
            if (response.data.isSuccess) {
                return response.data.content;
            }
        }
    }
    catch (error) {
        console.log(error);
    }

    return null;
};

const AuthService = {
    getAccessToken,
    generateDirectlineToken
}
export default AuthService;