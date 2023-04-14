import {Example} from "./Example";

import styles from "./Example.module.css";

export type ExampleModel = {
    text: string;
    value: string;
};

const EXAMPLES: ExampleModel[] = [
    {
        text: "怎么收集蓝屏dump方便CSS分析root cause？",
        value: "怎么收集蓝屏dump方便CSS分析root cause？"
    },
    {
        text: "我的3630惠普打印机如何连接WIFI？",
        value: "我的3630惠普打印机如何连接WIFI？"
    },
    {
        text: "Does my plan cover the annual eye exams?",
        value: "Does my plan cover the annual eye exams?"
    }
];

interface Props {
    onExampleClicked: (value: string) => void;
}

export const ExampleList = ({onExampleClicked}: Props) => {
    return (
        <ul className={styles.examplesNavList}>
            {EXAMPLES.map((x, i) => (
                <li key={i}>
                    <Example text={x.text} value={x.value} onClick={onExampleClicked}/>
                </li>
            ))}
        </ul>
    );
};
