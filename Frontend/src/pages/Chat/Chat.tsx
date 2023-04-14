import {useRef, useState, useEffect} from "react";
import {Checkbox, Panel, DefaultButton, TextField, SpinButton} from "@fluentui/react";
import {SparkleFilled} from "@fluentui/react-icons";

import styles from "./Chat.module.css";

import {chatApi, Approaches, AskResponse, ChatRequest, ChatTurn} from "../../api";
import {Answer, AnswerError, AnswerLoading} from "../../components/Answer";
import {QuestionInput} from "../../components/QuestionInput";
import {ExampleList} from "../../components/Example";
import {UserChatMessage} from "../../components/UserChatMessage";
import {AnalysisPanel, AnalysisPanelTabs} from "../../components/AnalysisPanel";
import {SettingsButton} from "../../components/SettingsButton";
import {ClearChatButton} from "../../components/ClearChatButton";


const handleSend = async (value: string) => {
    const response = await fetch("/getindex", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({myindex_selectedValue: value}),
    });

    if (response.ok) {
        console.log("Selected value sent to Flask successfully!");
    } else {
        console.error("Failed to send selected value to Flask.");
    }
};

const handleChange = (event: { target: { value: any; }; }) => {
    const value = event.target.value;
    handleSend(value);
};


const Chat = () => {
    const [isConfigPanelOpen, setIsConfigPanelOpen] = useState(false);
    const [promptTemplate, setPromptTemplate] = useState<string>("");
    const [retrieveCount, setRetrieveCount] = useState<number>(3);
    const [useSemanticRanker, setUseSemanticRanker] = useState<boolean>(true);
    const [myindex_selectedValue, setmyindex_selectedValue] = useState("");

    const [useSemanticCaptions, setUseSemanticCaptions] = useState<boolean>(false);
    const [excludeCategory, setExcludeCategory] = useState<string>("");
    const [useSuggestFollowupQuestions, setUseSuggestFollowupQuestions] = useState<boolean>(false);

    const lastQuestionRef = useRef<string>("");
    const chatMessageStreamEnd = useRef<HTMLDivElement | null>(null);

    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [error, setError] = useState<unknown>();

    const [activeCitation, setActiveCitation] = useState<string>();
    const [activeAnalysisPanelTab, setActiveAnalysisPanelTab] = useState<AnalysisPanelTabs | undefined>(undefined);

    const [selectedAnswer, setSelectedAnswer] = useState<number>(0);
    const [answers, setAnswers] = useState<[user: string, response: AskResponse][]>([]);

    const makeApiRequest = async (question: string, myindex_selectedValue: string) => {
        lastQuestionRef.current = question;

        error && setError(undefined);
        setIsLoading(true);
        setActiveCitation(undefined);
        setActiveAnalysisPanelTab(undefined);

        try {
            const history: ChatTurn[] = answers.map(a => ({user: a[0], bot: a[1].answer}));
            const request: ChatRequest = {
                history: [...history, {user: question, bot: undefined}],
                approach: Approaches.ReadRetrieveRead,
                overrides: {
                    promptTemplate: promptTemplate.length === 0 ? undefined : promptTemplate,
                    excludeCategory: excludeCategory.length === 0 ? undefined : excludeCategory,
                    top: retrieveCount,
                    semanticRanker: useSemanticRanker,

                    semanticCaptions: useSemanticCaptions,
                    suggestFollowupQuestions: useSuggestFollowupQuestions
                },
                myindex_selectedValue: myindex_selectedValue,
            };
            const result = await chatApi(request);
            setAnswers([...answers, [question, result]]);
        } catch (e) {
            setError(e);
        } finally {
            setIsLoading(false);
        }
    };

    const clearChat = () => {
        lastQuestionRef.current = "";
        error && setError(undefined);
        setActiveCitation(undefined);
        setActiveAnalysisPanelTab(undefined);
        setAnswers([]);
    };

    useEffect(() => chatMessageStreamEnd.current?.scrollIntoView({behavior: "smooth"}), [isLoading]);

    const onPromptTemplateChange = (_ev?: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
        setPromptTemplate(newValue || "");
    };

    const onRetrieveCountChange = (_ev?: React.SyntheticEvent<HTMLElement, Event>, newValue?: string) => {
        setRetrieveCount(parseInt(newValue || "3"));
    };

    const onUseSemanticRankerChange = (_ev?: React.FormEvent<HTMLElement | HTMLInputElement>, checked?: boolean) => {
        setUseSemanticRanker(!!checked);
    };

    const onmyindex_selectedValueChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
        setmyindex_selectedValue(event.target.value);
    };

    const onUseSemanticCaptionsChange = (_ev?: React.FormEvent<HTMLElement | HTMLInputElement>, checked?: boolean) => {
        setUseSemanticCaptions(!!checked);
    };


    const onExcludeCategoryChanged = (_ev?: React.FormEvent, newValue?: string) => {
        setExcludeCategory(newValue || "");
    };

    const onUseSuggestFollowupQuestionsChange = (_ev?: React.FormEvent<HTMLElement | HTMLInputElement>, checked?: boolean) => {
        setUseSuggestFollowupQuestions(!!checked);
    };

    const onExampleClicked = (example: string) => {
        makeApiRequest(example, myindex_selectedValue);
    };

    const onShowCitation = (citation: string, index: number) => {
        if (activeCitation === citation && activeAnalysisPanelTab === AnalysisPanelTabs.CitationTab && selectedAnswer === index) {
            setActiveAnalysisPanelTab(undefined);
        } else {
            setActiveCitation(citation);
            setActiveAnalysisPanelTab(AnalysisPanelTabs.CitationTab);
        }

        setSelectedAnswer(index);
    };

    const onToggleTab = (tab: AnalysisPanelTabs, index: number) => {
        if (activeAnalysisPanelTab === tab && selectedAnswer === index) {
            setActiveAnalysisPanelTab(undefined);
        } else {
            setActiveAnalysisPanelTab(tab);
        }

        setSelectedAnswer(index);
    };

    // @ts-ignore
    // @ts-ignore
    return (
        <div className={styles.container}>
            <div className={styles.commandsContainer}>
                <ClearChatButton className={styles.commandButton} onClick={clearChat}
                                 disabled={!lastQuestionRef.current || isLoading}/>
                <SettingsButton className={styles.commandButton}
                                onClick={() => setIsConfigPanelOpen(!isConfigPanelOpen)}/>
            </div>
            <div className={styles.chatRoot}>
                <div className={styles.chatContainer}>
                    {!lastQuestionRef.current ? (
                        <div className={styles.chatEmptyState}>
                            <SparkleFilled fontSize={"120px"} primaryFill={"rgba(115, 118, 225, 1)"}
                                           aria-hidden="true"
                                           aria-label="Chat logo"/>
                            <h1 className={styles.chatEmptyStateTitle}>你好，我是基于私域的Azure GPT</h1>
                            <h2 className={styles.chatEmptyStateSubtitle}>Ask anything or try an example</h2>
                            <ExampleList onExampleClicked={onExampleClicked}/>
                        </div>
                    ) : (
                        <div className={styles.chatMessageStream}>
                            {answers.map((answer, index) => (
                                <div key={index}>
                                    <UserChatMessage message={answer[0]}/>
                                    <div className={styles.chatMessageGpt}>
                                        <Answer
                                            key={index}
                                            answer={answer[1]}
                                            isSelected={selectedAnswer === index && activeAnalysisPanelTab !== undefined}
                                            onCitationClicked={(c:any) => onShowCitation(c, index)}
                                            onThoughtProcessClicked={() => onToggleTab(AnalysisPanelTabs.ThoughtProcessTab, index)}
                                            onSupportingContentClicked={() => onToggleTab(AnalysisPanelTabs.SupportingContentTab, index)}
                                            onFollowupQuestionClicked={(q:any)=> makeApiRequest(q, myindex_selectedValue)}
                                            showFollowupQuestions={useSuggestFollowupQuestions && answers.length - 1 === index}
                                        />
                                    </div>
                                </div>
                            ))}
                            {isLoading && (
                                <>
                                    <UserChatMessage message={lastQuestionRef.current}/>
                                    <div className={styles.chatMessageGptMinWidth}>
                                        <AnswerLoading/>
                                    </div>
                                </>
                            )}
                            {error ? (
                                <>
                                    <UserChatMessage message={lastQuestionRef.current}/>
                                    <div className={styles.chatMessageGptMinWidth}>
                                        <AnswerError error={error.toString()}
                                                     onRetry={() => makeApiRequest(lastQuestionRef.current, myindex_selectedValue)}/>
                                    </div>
                                </>
                            ) : null}
                            <div ref={chatMessageStreamEnd}/>
                        </div>
                    )}

                    <div className={styles.chatInput}>
                        <QuestionInput
                            clearOnSend
                            placeholder="Type a new question"
                            disabled={isLoading}
                            onSend={question => makeApiRequest(question, myindex_selectedValue)}
                        />
                    </div>
                </div>

                {answers.length > 0 && activeAnalysisPanelTab && (
                    <AnalysisPanel
                        className={styles.chatAnalysisPanel}
                        activeCitation={activeCitation}
                        onActiveTabChanged={x => onToggleTab(x, selectedAnswer)}
                        citationHeight="810px"
                        answer={answers[selectedAnswer][1]}
                        activeTab={activeAnalysisPanelTab}
                    />
                )}

                <Panel
                    headerText="选择对应的私域数据范围"
                    isOpen={isConfigPanelOpen}
                    isBlocking={false}
                    onDismiss={() => setIsConfigPanelOpen(false)}
                    closeButtonAriaLabel="Close"
                    onRenderFooterContent={() => <DefaultButton
                        onClick={() => setIsConfigPanelOpen(false)}>Close</DefaultButton>}
                    isFooterAtBottom={true}
                >
                    <div>
                        <select title='私域数据目录' defaultValue='kbindex' value={myindex_selectedValue} onChange={onmyindex_selectedValueChange} className={styles.select}>
                            <option value="kbindex" >知识库</option>
                        </select>
                    </div>
                </Panel>
            </div>
        </div>
    );
};

export default Chat;
