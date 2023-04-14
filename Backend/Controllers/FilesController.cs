using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Backend.Model;
using Backend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Services.Azure.Storage;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace WebApi.Controllers
{
    [Route("api/file")]
    [ApiController]
    public class FilesController : ControllerBase
    {


        private readonly ILogger<FilesController> _logger;
        private readonly IBlobStorageService _blobStorage;
        private readonly AIService _aiService;

        public FilesController(
            AIService aiService,
            IBlobStorageService blobStorage,
            ILogger<FilesController> logger)
        {
            _aiService= aiService;
            _blobStorage = blobStorage;
            _logger = logger;
        }

        [HttpPost, DisableRequestSizeLimit, RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [Route("upload")]
        public IActionResult Upload([FromForm] IFormCollection formData)
        {
            _logger.Enter();
            try
            {
                var files = formData.Files;
                var tasks = new List<Task<BlobContentInfo?>>();
                foreach (var item in files)
                {
                    tasks.Add(_blobStorage.UploadBlobAsync(item.OpenReadStream(), $"{item.FileName}", item.ContentType));
                }
                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (AggregateException ae)
                {
                    _logger.LogWarning("One or more exceptions occurred in UploadBlobAsync:");
                    foreach (var ex in ae.InnerExceptions)
                    {
                        _logger.LogWarning(ex, "Exception from UploadBlobAsync");
                    }
                    return StatusCode(500, "Internal server error");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload file failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }

        [HttpPost, DisableRequestSizeLimit, RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [Route("uploadPdf")]
        public async Task<IActionResult> UploadPdf([FromForm] IFormCollection formData)
        {
            _logger.Enter();
            try
            {

                await _aiService.CreateSearchIndexAsync();
                await _aiService.CreateCollectionAsync();
                var files = formData.Files;
                var tasks = new List<Task<BlobContentInfo?>>();
                foreach (var file in files)
                {
                    try
                    {
                        await _blobStorage.UploadBlobAsync(file.OpenReadStream(), $"/data/{file.FileName}", file.ContentType);

                    }
                    catch (AggregateException ae)
                    {
                        _logger.LogWarning("One or more exceptions occurred in UploadBlobAsync:");
                        foreach (var ex in ae.InnerExceptions)
                        {
                            _logger.LogWarning(ex, "Exception from UploadBlobAsync");
                        }
                        return StatusCode(500, "Internal server error");
                    }
                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(file.OpenReadStream());
                    // Loading page collections
                    PdfLoadedPageCollection loadedPages = loadedDocument.Pages;

                    var sections = CreateSections(file.FileName, loadedPages);
                    await _aiService.IndexDocumentsAsync(sections);
                    await _aiService.EmbeddingDocumentAsync(sections);
                    loadedDocument.Close(true);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload file failed");
                return StatusCode(500, ex.Message);
            }
            finally
            {
                _logger.Exit();
            }
        }

        private static Stream ImportPageToPdf(PdfLoadedDocument loadedDocument, int index)
        {
            PdfDocument document = new PdfDocument();
            document.ImportPage(loadedDocument, index);

            //Creating the stream object
            MemoryStream stream = new MemoryStream();
            //Save the document into memory stream
            document.Save(stream);
            //If the position is not set to '0' then the PDF will be empty.
            stream.Position = 0;
            //Close the document.
            document.Close(true);
            return stream;
        }


        private Stream WriteToPdf(string content)
        {
            PdfDocument document = new PdfDocument();

            //Add a page to the document.
            PdfPage page = document.Pages.Add();

            //Create PDF graphics for the page.
            PdfGraphics graphics = page.Graphics;

            PdfTextElement element = new PdfTextElement(content);
            element.Brush = new PdfSolidBrush(Color.Black);
            element.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
            //Set the properties to paginate the text
            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Break = PdfLayoutBreakType.FitPage;
            //Set bounds to draw multiline text
            RectangleF bounds = new RectangleF(PointF.Empty, page.Graphics.ClientSize);
            //Draw the text element with the properties and formats set
            element.Draw(page, bounds, layoutFormat);

            //Creating the stream object
            MemoryStream stream = new MemoryStream();
            //Save the document into memory stream
            document.Save(stream);
            //If the position is not set to '0' then the PDF will be empty.
            stream.Position = 0;
            //Close the document.
            document.Close(true);
            return stream;
        }


        private IEnumerable<(string, int)> SplitTextByLanguage(PdfLoadedPageCollection pages, string language)
        {
            int MAX_SECTION_LENGTH = language == "cn" ? 400 : 1000;
            int SECTION_OVERLAP = language == "cn" ? 50 : 100;
            int SENTENCE_SEARCH_LIMIT = language == "cn" ? 50 : 100;

            string[] SENTENCE_ENDINGS = language == "cn" ?
                new[] { "。", "！", "？" }
                : new[] { ".", "!", "?", "。", "！", "？" };
            string[] WORDS_BREAKS = language == "cn" ?
                new[] { "，", "；", "：", " ", "　", "（", "）", "【", "】", "{", "｛", "}", "｝", "\t", "\n" }
                : new[] { ",", ";", ":", " ", "(", ")", "[", "]", "{", "}", "\t", "\n" };

            var pageMap = new List<(int index, int offset, string content)>();
            int offset = 0;
            for (int i = 0; i < pages.Count; i++)
            {
                string text = pages[i].ExtractText(true);
                pageMap.Add((i, offset, text));
                offset += text.Length;
            }
            int length = offset;
            string allText = string.Join("", pageMap.Select(p => p.content));
            int start = 0;
            int end = length;
            while (start + SECTION_OVERLAP < length)
            {
                int lastWord = -1;
                end = start + MAX_SECTION_LENGTH;
                if (end > length)
                {
                    end = length;
                }
                else
                {
                    // Try to find the end of the sentence
                    while (end < length && (end - start - MAX_SECTION_LENGTH) < SENTENCE_SEARCH_LIMIT && !SENTENCE_ENDINGS.Contains(allText[end].ToString()))
                    {
                        if (WORDS_BREAKS.Contains(allText[end].ToString()))
                        {
                            lastWord = end;
                        }
                        end++;
                    }
                    if (end < length && !SENTENCE_ENDINGS.Contains(allText[end].ToString()) && lastWord > 0)
                    {
                        end = lastWord; // Fall back to at least keeping a whole word
                    }
                }
                if (end < length)
                {
                    end++;
                }

                // Try to find the start of the sentence or at least a whole word boundary
                lastWord = -1;
                while (start > 0 && start > end - MAX_SECTION_LENGTH - 2 * SENTENCE_SEARCH_LIMIT && !SENTENCE_ENDINGS.Contains(allText[start].ToString()))
                {
                    if (WORDS_BREAKS.Contains(allText[start].ToString()))
                    {
                        lastWord = start;
                    }
                    start--;
                }
                if (!SENTENCE_ENDINGS.Contains(allText[start].ToString()) && lastWord > 0)
                {
                    start = lastWord;
                }
                if (start > 0)
                {
                    start++;
                }

                yield return (allText.Substring(start, end - start), FindPage(start, pageMap));
                start = end - SECTION_OVERLAP;
            }
            if (start + SECTION_OVERLAP < end)
            {
                yield return (allText.Substring(start, end - start), FindPage(start, pageMap));
            }
        }

        private IEnumerable<(string, int)> SplitText(PdfLoadedPageCollection pages)
        {
            return SplitTextByLanguage(pages, "en");
        }

        private IEnumerable<(string, int)> SplitChineseText(PdfLoadedPageCollection pages)
        {
            return SplitTextByLanguage(pages, "cn");
        }

        private int FindPage(int offset, List<(int index, int offset, string content)> pageMap)
        {
            for (int i = 0; i < pageMap.Count - 1; i++)
            {
                if (offset >= pageMap[i].offset && offset < pageMap[i + 1].offset)
                {
                    return pageMap[i].index;
                }
            }
            return pageMap.Last().index;
        }

        private IEnumerable<Dictionary<string, string>> CreateSections(string filename, PdfLoadedPageCollection pages, string category = "")
        {
            int i = 0;
            var contentName = "content_en";
            var language = "en";
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filename);
            if (fileNameWithoutExt.Substring(fileNameWithoutExt.Length - 3)=="-cn")
            {
                contentName="content_cn";
                language="cn";
            }
            if (language=="cn")
            {
                foreach ((string section, int pagenum) in SplitChineseText(pages))
                {
                    yield return new Dictionary<string, string>
                    {
                        { "id", $"{ filename }-{ i }".Replace(".", "").Replace(" ", "") },
                        { contentName, section },
                        { "category", category },
                        { "sourcepage", $"{fileNameWithoutExt}{pagenum}.pdf" },
                        { "sourcefile", filename }
                    };
                    i++;
                }
            }
            else
            {
                foreach ((string section, int pagenum) in SplitText(pages))
                {
                    yield return new Dictionary<string, string>
                {
                        { "id", $"{ filename }-{ i }".Replace(".", "").Replace(" ", "") },
                        { contentName, section },
                        { "category", category },
                        { "sourcepage", $"{fileNameWithoutExt}{pagenum}.pdf" },
                        { "sourcefile", filename }
                    };
                    i++;
                }
            }

        }



        private static (string output, string remain) SplitString(string input)
        {
            const int MinStringLength = 500;
            const int MaxStringLength = 1000;
            //List<string> output = new List<string>();
            string output = string.Empty;
            // Use regular expressions to split the string
            string pattern = @"([\S ]+?[.!?������][""��]{0,1}\r{0,1}\n{0,1})";
            string[] split = Regex.Split(input, pattern, RegexOptions.Multiline);
            int sentenseIndex = 0;
            string sentenseOutput = string.Empty;
            int sectionIndex = 0;
            string sectionOutput = string.Empty;
            // Loop through the split strings and add them to the output list
            for (int i = 0; i < split.Length; i++)
            {
                string current = split[i].Trim();


                // If the string is not empty, add it to the output list
                if (!string.IsNullOrEmpty(current))
                {
                    // If the current string is not the first one, add the separator to the previous string
                    if (i > 0)
                    {
                        if ((sentenseOutput+split[i]).Length>MaxStringLength)
                        {
                            break;
                        }

                        //it's a section break;
                        if (split[i].EndsWith('\n'))
                        {
                            sectionOutput=sentenseOutput+split[i];
                            sectionIndex=i;
                        }
                    }

                    sentenseOutput+=split[i];
                    sentenseIndex=i;

                }
            }

            if (sectionOutput.Length>MinStringLength)
            {
                return (sectionOutput, string.Join(" ", split, sectionIndex+1, split.Length-sectionIndex-1));
            }
            else
            {
                if (sentenseOutput.Length>MinStringLength)
                {
                    return (sentenseOutput, string.Join(" ", split, sentenseIndex+1, split.Length-sentenseIndex-1));
                }
                else
                {
                    return (string.Empty, input);
                }
            }
        }

        private static List<string> SplitText(string text, int maxStringLength)
        {
            List<string> result = new List<string>();

            while (text.Length > maxStringLength)
            {
                int index = text.LastIndexOfAny(new char[] { '.', ';', '!', '?' }, maxStringLength - 1);
                if (index == -1)
                {
                    index = maxStringLength;
                }
                result.Add(text.Substring(0, index).Trim());
                text = text.Substring(index).TrimStart();
            }

            if (text.Length > 0)
            {
                result.Add(text);
            }

            return result;
        }

        [HttpGet, DisableRequestSizeLimit]
        [Route("download")]
        public async Task<IActionResult> Download([FromQuery] string filePath)
        {
            _logger.Enter();
            await Task.CompletedTask;
            try
            {
                var stream = await _blobStorage.GetBlobAsync(filePath);
                var properties = await _blobStorage.GetBlobPropertiesAsync(filePath);
                if (stream!=null && properties!=null)
                {
                    return File(stream, properties.ContentType);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Download file failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }

        [HttpGet]
        [Route("getuploaduri")]
        public async Task<IActionResult> GetUploadUri([FromQuery] string filePath, int period = 60)
        {
            _logger.Enter();

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogInformation("filePath is null in GetUploadUri");
                    return BadRequest();
                }

                var expireTime = DateTime.Now.AddMinutes(period);
                var uri = await _blobStorage.GetBlobUriAsync(filePath, expireTime, BlobSasPermissions.Create |BlobSasPermissions.Add | BlobSasPermissions.Read | BlobSasPermissions.Write);
                if (!string.IsNullOrEmpty(uri?.ToString()))
                {
                    return Ok(new
                    {
                        isSuccess = true,
                        Content = new
                        {
                            Uri = uri
                        }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        isSuccess = false,
                        Message = "Can't get SAS Uri"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUploadUri failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }

        [HttpGet]
        [Route("getdownloaduri")]
        public async Task<IActionResult> GetDownloadUri([FromQuery] string filePath, int period = 120)
        {
            _logger.Enter();

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogInformation("filePath is null in GetUploadUri");
                    return BadRequest();
                }

                var expireTime = DateTime.Now.AddMinutes(period);
                var uri = await _blobStorage.GetBlobUriAsync(filePath, expireTime, BlobSasPermissions.Read);
                if (!string.IsNullOrEmpty(uri?.ToString()))
                {
                    return Ok(new
                    {
                        isSuccess = true,
                        Content = new
                        {
                            Uri = uri
                        }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        isSuccess = false,
                        Message = "Can't get SAS Uri"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDownloadUri failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }


        [HttpPost]
        [Route("list")]
        public async Task<IActionResult> List([FromBody] ListModel listModel)
        {
            _logger.Enter();
            try
            {
                var prefix = listModel.Prefix;
                dynamic? result = null;
                if (!string.IsNullOrEmpty(prefix) && (prefix[0] == '/' || prefix[0] == '\\'))
                {
                    string trimmedPath = prefix.TrimStart('/', '\\');
                    List<string> pathItems = trimmedPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var count = pathItems.Count();
                    if (count > 1)
                    {
                        var containerName = pathItems[0];

                        pathItems.RemoveAt(0);
                        var pathWithoutContainerName = String.Join('/', pathItems.ToArray());

                        var blobs = await _blobStorage.GetBlobsAsync(containerName, pathWithoutContainerName+"/");
                        if (blobs!=null)
                        {
                            result = await (from item in blobs
                                            select new { Name = item.Name }).ToArrayAsync();

                        }
                        else
                        {
                            _logger.LogInformation("No files found");
                            return NotFound();
                        }
                    }
                    else if (count==1)
                    {
                        var containerName = pathItems[0];
                        var blobs = await _blobStorage.GetBlobsAsync(containerName);
                        if (blobs!=null)
                        {
                            result = await (from item in blobs
                                            select new { Name = item.Name }).ToArrayAsync();

                        }
                        else
                        {
                            _logger.LogInformation("No files found");
                            return NotFound();
                        }
                    }
                }
                else
                {
                    var blobs = await _blobStorage.GetBlobsAsync(null, prefix+"/");
                    if (blobs != null)
                    {
                        result = await (from item in blobs
                                        select new { Name = item.Name }).ToArrayAsync();
                    }
                    else
                    {
                        _logger.LogInformation("No files found");
                        return NotFound();
                    }
                }
                if (result != null)
                {

                    return Ok(new
                    {
                        isSuccess = true,
                        Content = result
                    });
                }
                else
                {
                    return Ok(new
                    {
                        isSuccess = false,
                        Message = "Cant get blob list in specified path"
                    });
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "List files failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }


        [HttpGet]
        [Route("list")]
        public async Task<IActionResult> List()
        {
            _logger.Enter();
            try
            {
                var blobs = await _blobStorage.GetBlobsAsync();
                if (blobs != null)
                {
                    var result = await (from item in blobs
                                        select new { Name = item.Name, Uri = _blobStorage.GetBlobUriAsync($"{item.Name}").Result }).ToArrayAsync();

                    // Response.StatusCode = StatusCodes.Status400BadRequest;
                    return new JsonResult(result);
                }
                _logger.LogInformation("No files found");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "List files failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }


        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteModel deleteModel)
        {
            _logger.Enter();
            try
            {
                var name = deleteModel.Name;
                var result = await _blobStorage.DeleteAsync(name);
                if (name.StartsWith("/data/"))
                {
                    var fileName = Path.GetFileName(name);
                    await _aiService.RemoveFileFromIndex(fileName);
                    await _aiService.DeletePoints(fileName);
                }
                return Ok(new
                {
                    isSuccess = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "List files failed");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                _logger.Exit();
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}