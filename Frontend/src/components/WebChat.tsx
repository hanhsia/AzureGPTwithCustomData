import { useState, useEffect, useCallback } from 'react';
import ReactWebChat, { createDirectLine, createDirectLineAppServiceExtension, createStore, createStyleSet } from 'botframework-webchat';
import { ConnectionStatus, DirectLine } from 'botframework-directlinejs';
import axios from "axios";
import { SeverityLevel } from '@microsoft/applicationinsights-web';

import AuthService from "../services/auth.service"


function generateRandomUsername(): string {
    const alphabet = 'abcdefghijklmnopqrstuvwxyz';
    const upperCaseAlphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const numbers = '0123456789';

    let username = '';

    // First letter is uppercase
    username += upperCaseAlphabet.charAt(Math.floor(Math.random() * upperCaseAlphabet.length));

    // Middle four letters are lowercase
    for (let i = 0; i < 5; i++) {
        username += alphabet.charAt(Math.floor(Math.random() * alphabet.length));
    }

    // Last five letters are numbers
    for (let i = 0; i < 4; i++) {
        username += numbers.charAt(Math.floor(Math.random() * numbers.length));
    }

    return username;
}



const WebChat = (props: any) => {
    const [loading, setLoading] = useState(true);
    const [userID, setUserID] = useState<string>("");
    const [store, setStore] = useState<any>();
    const [directLine, setDirectLine] = useState<any>(null);

    const webChatStore = createStore({}, (dispatch: any) => (next: any) => (action: any) => {
        connectionStatusChanged(directLine.connectionStatus$);
        if (action.type === 'DIRECT_LINE/CONNECT_FULFILLED') {
            dispatch({
                type: 'WEB_CHAT/SEND_EVENT',
                payload: {
                    name: 'webchat/join',
                },
            });
            console.log('Join')
        }

        if (action.type === 'DIRECT_LINE/INCOMING_ACTIVITY') {
            console.log('INCOMING ACTIVITY ', action.payload.activity);
        }
        return next(action);
    });


    const connectionStatusChanged = (connectionStatus: any) => {
        switch (connectionStatus.value) {
            case ConnectionStatus.Online:
                console.log('Connected')
        }
    };

    const styleSet = createStyleSet({
        bubbleBackground: 'rgba(0, 0, 255, .1)',
        bubbleFromUserBackground: 'rgba(0, 255, 0, .1)',
        rootHeight: '100%',
        rootWidth: '100%',
        bubbleMaxWidth: 600,
        backgroundColor: 'paleturquoise'
    });

    const avatarOptions = {
        botAvatarInitials: 'GPT',
        userAvatarInitials: "ME"
    };

    const getDirectLine = useCallback(async () => {


        async function getDirectLineToken(): Promise<string | null> {
            let response: any = null;
            let conversationId = sessionStorage.getItem("conversationId");
            let directLineToken = sessionStorage.getItem("directLineToken");


            if (conversationId == null || directLineToken == null) {
                response = await AuthService.generateDirectlineToken();
                if (response != null) {
                    sessionStorage.setItem("conversationId", response.conversationId);
                    sessionStorage.setItem("directLineToken", response.token);
                    directLineToken = response.token;
                }
            }
            else {
                try {
                    //get new token according to conversationId so that we can keep the chat history.
                    response = await axios.get(
                        `${process.env.REACT_APP_DIRECTLINE_URL}/conversations/${conversationId}`,
                        {
                            headers: { Authorization: 'Bearer ' + directLineToken }
                        });
                    if (response.status === 200) {
                        sessionStorage.setItem("directLineToken", response.data.token);
                        directLineToken = response.data.token;
                    }
                }
                catch (error) {
                    console.log(error);
                    directLineToken = null;
                }
            }
            return directLineToken;
        }

        async function createDirectLineObject(directLineToken: string): Promise<boolean> {
            try {
                let directLineObject =
                    createDirectLine(
                        {
                            domain: process.env.REACT_APP_DIRECTLINE_URL,
                            token: directLineToken
                        })
                setDirectLine(directLineObject);
                setLoading(false);

                return true;
            }
            catch (error) {
                console.log(error);
            }
            return false;
        }


        let directLineToken = await getDirectLineToken();

        //create directline object
        if (directLineToken != null) {
            let result = await createDirectLineObject(directLineToken);
            if (result) {
                return;
            }
        }

        sessionStorage.removeItem("conversationId");
        sessionStorage.removeItem("directLineToken");
        //try again when encounter any failure.

        directLineToken = await getDirectLineToken();

        //create directline object again
        if (directLineToken != null) {
            await createDirectLineObject(directLineToken);
        }



    }, [setDirectLine]);


    useEffect(() => {

        if (loading === false) {
            setStore(webChatStore);
        }
        if (loading === true) {


            let id = sessionStorage.getItem("userId");
            if (id == null) {
                id = generateRandomUsername();
                sessionStorage.setItem("userId", id);
            }
            setUserID(id);
            getDirectLine();
        }
    }, [getDirectLine, setUserID]);

    return loading === false && !!directLine ? (
        <ReactWebChat
            directLine={directLine}
            userID={userID}
            username={userID}
            store={store}
            styleSet={styleSet}
            styleOptions={avatarOptions}
        />
    ) : (
        <div>Loading...</div>
    );
};

export default WebChat;