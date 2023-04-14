import axios from "axios";

const API_URL = process.env.REACT_APP_DOTNET_API_PATH;

const getResponse = async (token: string, style:string, question: string, history: any): Promise<string | null> => {
    try {
        
        let response = await axios.post(
            API_URL + "/api/openai/answer",
            {
                style:style,
                question: question,
                history:history
            }
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

const getStreamResponse = async (
    token: string,
    question: string,
    history: any
): Promise<EventSource | null> => {
    try {
        const response = await fetch(API_URL + "/api/openai/streamAnswer", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                question: question,
                history: history,
            }),
        });

        if (response.status === 200) {
            const eventSourceUrl = response.url + "?access_token=" + token;
            const eventSource = new EventSource(eventSourceUrl);
            return eventSource;
        }
    } catch (error) {
        console.log(error);
    }

    return null;
};


const OpenAIService = {
    getResponse,
    getStreamResponse

}
export default OpenAIService;