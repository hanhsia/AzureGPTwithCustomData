import { Pivot, PivotItem } from "@fluentui/react";
import DOMPurify from "dompurify";

import styles from "./AnalysisPanel.module.css";

import { SupportingContent } from "../SupportingContent";
import { AskResponse } from "../../api";
import { AnalysisPanelTabs } from "./AnalysisPanelTabs";

interface Props {
    className: string;
    activeTab: AnalysisPanelTabs;
    onActiveTabChanged: (tab: AnalysisPanelTabs) => void;
    activeCitation: string | undefined;
    citationHeight: string;
    answer: AskResponse;
}

const pivotItemDisabledStyle = { disabled: true, style: { color: "grey" } };

export const AnalysisPanel = ({ answer, activeTab, activeCitation, citationHeight, className, onActiveTabChanged }: Props) => {
    // const isDisabledThoughtProcessTab: boolean = !answer.thoughts;
    // const isDisabledSupportingContentTab: boolean = !answer.data_points.length;
    // const isDisabledCitationTab: boolean = !activeCitation;

    const isDisabledThoughtProcessTab: boolean = true;
    const isDisabledSupportingContentTab: boolean = true;
    const isDisabledCitationTab: boolean = true;

    const sanitizedThoughts = DOMPurify.sanitize(answer.thoughts!);

    return (
        <Pivot
            className={className}
            selectedKey={activeTab}
            onLinkClick={pivotItem => pivotItem && onActiveTabChanged(pivotItem.props.itemKey! as AnalysisPanelTabs)}
        >

        </Pivot>
    );
};
