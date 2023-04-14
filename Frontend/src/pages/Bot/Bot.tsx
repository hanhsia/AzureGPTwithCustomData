import { useState, useEffect} from 'react';
import { Box, Stack} from "@mui/material";
import { Chat, DriveFileMove, RateReview } from "@mui/icons-material";
import WebChat from "../../components/WebChat";
import Sidebar from "../../components/Sidebar";


const Main = (props: any) => {
    
    switch (props.currentItem) {
        case 'Chat':
            return (
                <>
                    <Box sx={{ flexGrow: 1 }} >
                            {<WebChat />}
                        </Box >
                </>
            );
        default: return null;
    }

}

const Bot = () => {

    const [currentItem, setCurrentItem] = useState<string>("Chat");

    useEffect(() => {
        let item = sessionStorage.getItem("currentHomeItem")
        if (item != null) {
            setCurrentItem(item);
        }
    }, [setCurrentItem]);

    const items = [
        {
            id: 'Chat',
            title: 'GPT Bot',
            icon: <Chat />,
        }
    ];
    const handleItemClick = (itemId: string) => {
        setCurrentItem(itemId);
        sessionStorage.setItem('currentHomeItem', itemId);
    };
    return (
        <Stack sx={{ flexGrow: 1, display: 'flex' }} direction="row">
            <Sidebar
                items={items}
                currentItem={currentItem}
                handleItemClick={handleItemClick}
            />
            <Main currentItem={currentItem} />
        </Stack>
    );
};

export default Bot;