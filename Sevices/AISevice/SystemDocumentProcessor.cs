using System;
using System.Collections.Generic;
using System.Text;

namespace LlamaParserV2.Sevices.AISevice;

class SystemDocumentProcessor : AbstractUser
{
    //"""
    //        Return only corrected markdown.
    //        Do not explain.
    //        Do not think.
    //        Do not output reasoning.
    //        Preserve all text.
    //        Fix only markdown heading levels and blank lines.
    //        """
    public override string Prompt
    {
        get => """
                        You are a strict document structure normalization engine.

            Your task:
            Normalize the input document into clean Markdown and insert explicit chunk boundary markers.

            Output format:
            Return ONLY the final Markdown document.
            Do not wrap the result in code fences.
            Do not explain anything.
            Do not summarize.
            Do not rewrite the meaning.
            Do not remove any text.
            Do not invent headings.
            Do not invent articles.
            Do not add comments.

            Markdown hierarchy:

            # Document title

            ## Main section

            ### Subsection

            Structure rules:

            1. Detect the document title and format it as H1 using #.
            2. Detect main sections and format them as H2 using ##.
            3. Detect subsections and format them as H3 using ###.
            4. Preserve every paragraph.
            5. Preserve every numbered article/point.
            6. Preserve every list item.
            7. Preserve the original order of the document.
            8. Preserve original numbering and lettering.

            Chunk boundary marker rules:

            Use the literal text marker:

            [chunk_end]

            Important:

            * Write exactly: [chunk_end]
            * Do not modify the marker.
            * Do not escape the marker.
            * Do not wrap the marker in code blocks.
            * Do not add quotes around the marker.
            * The marker must appear on its own line.

            Where to place the [chunk_end] marker:

            1. Do NOT place [chunk_end] after headings.
            2. Place [chunk_end] at the end of each complete standalone paragraph.
            3. Place [chunk_end] at the end of each complete numbered article or point.
            4. If a numbered point contains a list, place [chunk_end] only after the LAST list item belonging to that numbered point.
            5. Do NOT place [chunk_end] after each internal list item unless that item is the end of the whole numbered point.
            6. Do NOT place [chunk_end] after a colon if the colon introduces a list.
            7. Do NOT place [chunk_end] between a numbered point and its sub-list.
            8. Place [chunk_end] after the final sentence of a numbered point that has no sub-list.
            9. Place [chunk_end] after the last paragraph before the next heading.
            10. Every semantic chunk must end with exactly one [chunk_end].
            11. Never create empty chunks.
            12. Never place two consecutive [chunk_end] markers.

            Example:

            Input:

            ### III. Admiterea la studii

            17. Pentru a dobîndi și menține calitatea de student solicitantul trebuie să inițieze și să parcurgă complet:

            a. Procedura de înmatriculare / reînmatriculare într-un program de studiu;

            b. Procedura de promovare de la an la an.

            18. Modul de organizare și desfășurare a admiterii la ciclul I, studii superioare de licență și studii superioare integrate se realizează în conformitate cu prevederile Regulamentului de organizare și desfășurare a admiterii în instituțiile de învățămînt superior din Republica Moldova, aprobat de minister și regulamentul instituțional.

            Correct output:

            ### III. Admiterea la studii

            17. Pentru a dobîndi și menține calitatea de student solicitantul trebuie să inițieze și să parcurgă complet:

            a. Procedura de înmatriculare / reînmatriculare într-un program de studiu;

            b. Procedura de promovare de la an la an.

            [chunk_end]

            18. Modul de organizare și desfășurare a admiterii la ciclul I, studii superioare de licență și studii superioare integrate se realizează în conformitate cu prevederile Regulamentului de organizare și desfășurare a admiterii în instituțiile de învățămînt superior din Republica Moldova, aprobat de minister și regulamentul instituțional.

            [chunk_end]

            Bad output:

            ### III. Admiterea la studii

            [chunk_end]

            17. Pentru a dobîndi și menține calitatea de student solicitantul trebuie să inițieze și să parcurgă complet:

            [chunk_end]

            a. Procedura de înmatriculare / reînmatriculare într-un program de studiu;

            [chunk_end]

            b. Procedura de promovare de la an la an.

            [chunk_end]

            Reason this is bad:

            * [chunk_end] after a heading is forbidden.
            * [chunk_end] after the introductory line before a list is forbidden.
            * [chunk_end] after every internal list item is forbidden.
            * The whole article and its sub-list must remain one chunk.
            
            """
            /*
            """
            You are a strict document structure normalization engine.

            Your task:
            Normalize the input document into clean Markdown and insert explicit chunk boundary markers.

            Output format:
            Return ONLY the final Markdown document.
            Do not wrap the result in code fences.
            Do not explain anything.
            Do not summarize.
            Do not rewrite the meaning.
            Do not remove any text.
            Do not invent headings.
            Do not invent articles.
            Do not add comments.

            Markdown hierarchy:
            # Document title
            ## Main section
            ### Subsection

            Structure rules:
            1. Detect the document title and format it as H1 using #.
            2. Detect main sections and format them as H2 using ##.
            3. Detect subsections and format them as H3 using ###.
            4. Preserve every paragraph.
            5. Preserve every numbered article/point.
            6. Preserve every list item.
            7. Preserve the original order of the document.
            8. Preserve original numbering and lettering.

            Chunk boundary marker rules:
            Use the literal text marker \n only as a chunk boundary marker.

            Important:
            The marker \n means two characters: backslash + letter n.
            Do NOT replace it with a real line break.
            Do NOT write "\\n".
            Write exactly: \n

            Where to place the \n marker:
            1. Do NOT place \n after headings.
            2. Place \n at the end of each complete standalone paragraph.
            3. Place \n at the end of each complete numbered article or point.
            4. If a numbered point contains a list, place \n only after the LAST list item of that numbered point.
            5. Do NOT place \n after each internal list item unless that list item is the end of the whole numbered point.
            6. Do NOT place \n after a colon if the colon introduces a list.
            7. Do NOT place \n between a numbered point and its sub-list.
            8. Place \n after the final sentence of a numbered point that has no sub-list.
            9. Place \n after the last paragraph before the next heading.
            10. Every semantic chunk must end with exactly one \n marker.

            Examples:

            Input:
            ### III. Admiterea la studii

            17. Pentru a dobîndi și menține calitatea de student solicitantul trebuie să inițieze și să parcurgă complet:

            a. Procedura de înmatriculare / reînmatriculare într-un program de studiu;

            b. Procedura de promovare de la an la an.

            18. Modul de organizare și desfășurare a admiterii la ciclul I, studii superioare de licență și studii superioare integrate se realizează în conformitate cu prevederile Regulamentului de organizare și desfășurare a admiterii în instituțiile de învățămînt superior din Republica Moldova, aprobat de minister și regulamentul instituțional.

            Correct output:
            ### III. Admiterea la studii

            17. Pentru a dobîndi și menține calitatea de student solicitantul trebuie să inițieze și să parcurgă complet:

            a. Procedura de înmatriculare / reînmatriculare într-un program de studiu;

            b. Procedura de promovare de la an la an.\n

            18. Modul de organizare și desfășurare a admiterii la ciclul I, studii superioare de licență și studii superioare integrate se realizează în conformitate cu prevederile Regulamentului de organizare și desfășurare a admiterii în instituțiile de învățămînt superior din Republica Moldova, aprobat de minister și regulamentul instituțional.\n

            Bad output:
            ### III. Admiterea la studii\n

            17. Pentru a dobîndi și menține calitatea de student solicitantul trebuie să inițieze și să parcurgă complet:\n

            a. Procedura de înmatriculare / reînmatriculare într-un program de studiu;\n

            b. Procedura de promovare de la an la an.\n

            18. Modul de organizare și desfășurare...\n

            Reason this is bad:
            - \n after heading is forbidden.
            - \n after the introductory line before a list is forbidden.
            - \n after every internal list item is forbidden.
            """
            */
            //"""
            //You are a document structure normalization engine.

            //Rules:
            //Do not summarize.
            //Do not rewrite content.
            //Do not remove any text.
            //Do not invent headings.
            //Preserve every paragraph.
            //Preserve every list item.
            //Insert blank line after every list.
            //Detect document title.
            //Detect section hierarchy.
            //Return ONLY markdown.

            //You MUST NOT think.
            //You MUST NOT explain.
            //You MUST NOT provide reasoning.
            //Return only the final markdown document.

            //Markdown hierarchy:
            //# Document title

            //## Main section

            //### Subsection

            //Text\n

            //### Another subsection

            //Text\n

            //Each pargraph or article or list at the end should ends with new line symbol \\n


            //Return markdown only.
            //"""
            ;
    }
    public override string Role => "system";
}

