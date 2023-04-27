import { useState, useEffect, Fragment } from 'react';
import { Box, Typography, Stack, Button, List, ListItem, TextField} from "@mui/material";
import { FileUpload, Edit } from "@mui/icons-material";
import FilesUploader from "../../components/FilesUploader";
import Sidebar from "../../components/Sidebar";

const Main = (props: any) => {
    const [content, setContent] = useState<string>("");

    const handleChange = (content: string) => {
        setContent(content);
    };  
    const handleSubmit = () => {
        console.log("Submitted content:", content);
        setContent("test");
        // Add your submit logic here  
    };
    switch (props.currentItem) {
        case 'File':
            return (
                <Fragment>

                        <Box sx={{ flexGrow: 1 }} >
                        <Typography variant='subtitle1' textAlign={"center"} >知识库</Typography>
                        <Typography variant="h6">Notes:Please add a "-cn" suffix in your file name if it's a Chinese document,e.g. "sample-cn.pdf" </Typography>
                        <FilesUploader type={1} />
                        </Box >
                </Fragment>
            );
        // case 'Editor':
        //     return (
        //         <Fragment>
        //                 <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }} >
        //                     <Typography>KB Content</Typography>
        //                     <TextField multiline rows={8}
        //                         onChange={(event:any) => handleChange(event.target.value)}
        //                         value={content}
        //                     />
        //                     <Button
        //                         variant="contained"
        //                         color="primary"
        //                         onClick={handleSubmit}
        //                         sx={{ mt: 2 }}>
        //                         Submit
        //                     </Button>
        //                     <List sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' } }>
        //                         <ListItem>Comming soon</ListItem>
        //                     </List>
        //                 </Box >

        //         </Fragment>
        //     );
        default: return null;
    }
}

const Admin = () => {
    const [currentItem, setCurrentItem] = useState<string>("File");

    useEffect(() => {
        let item = sessionStorage.getItem("currentAdminItem")
        if (item != null) {
            setCurrentItem(item);
        }
    }, [setCurrentItem]);

    const items = [
        {
            id: 'File',
            title: 'KB Import',
            icon: <FileUpload />,
        },
        // {
        //     id: 'Editor',
        //     title: 'KB Editor',
        //     icon: <Edit />,
        // },
    ];
    const handleItemClick = (itemId: string) => {
        setCurrentItem(itemId);
        sessionStorage.setItem('currentAdminItem', itemId);
    };

    return (
        <Fragment>
                <Stack sx={{ flexGrow: 1, display: 'flex' }} direction="row"  >
                    <Sidebar
                        items={items}
                        currentItem={currentItem}
                        handleItemClick={handleItemClick}
                    />
                    <Main currentItem={currentItem} />
                </Stack>
        </Fragment>
    );
};

export default Admin;