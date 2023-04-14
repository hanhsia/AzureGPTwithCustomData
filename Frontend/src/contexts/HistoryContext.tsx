import { createContext} from "react";

interface State {
    isLoading: boolean;
    answerStyle: string;
    currentPage: number;
    question: string;
    history: { user: string; assistant: string }[];
    reserveHistory: boolean;
}

type Action =
    | { type: "SET_LOADING"; payload: boolean }
    | { type: "SET_ANSWER_STYLE"; payload: string }
    | { type: "SET_CURRENT_PAGE"; payload: number }
    | { type: "SET_QUESTION"; payload: string }
    | { type: "SET_HISTORY"; payload: { user: string; assistant: string }[] }
    | { type: "UPDATE_LAST_History"; payload: string }
    | { type: "SET_RESERVE_HISTORY"; payload: boolean };


interface HistoryContextType {
    state: State;
    dispatch: React.Dispatch<Action>;
}

export const initialState: State = {
    isLoading: false,
    answerStyle: "Coder",
    currentPage: 1,
    question: "",
    history: [],
    reserveHistory: true,
};



export const reducer = (state: State, action: Action): State => {
    switch (action.type) {
        case "SET_LOADING":
            return { ...state, isLoading: action.payload };
        case "SET_ANSWER_STYLE":
            return { ...state, answerStyle: action.payload };
        case "SET_CURRENT_PAGE":
            return { ...state, currentPage: action.payload };
        case "SET_QUESTION":
            return { ...state, question: action.payload };
        case "SET_HISTORY":
            return { ...state, history: action.payload };
        case "UPDATE_LAST_History":
            const updatedHistory = [...state.history];
            if (updatedHistory.length > 0) {
                updatedHistory[updatedHistory.length - 1] = {
                    user: updatedHistory[updatedHistory.length - 1].user,
                    assistant: updatedHistory[updatedHistory.length - 1].assistant + action.payload
                };
            }
            return { ...state, history: updatedHistory };
        case "SET_RESERVE_HISTORY":
            return { ...state, reserveHistory: action.payload };
        default:
            return state;
    }
};

const HistoryContext = createContext<HistoryContextType>({
    state: initialState,
    dispatch: () => { },
});


export default HistoryContext;